using System.Globalization;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

public sealed class AdminInsightsService(
    IAdminReportingRepository reporting,
    IFraudSignalRepository fraudSignals,
    IAuditLogRepository auditLog,
    IPlatformSettingsService settings,
    TimeProvider timeProvider) : IAdminInsightsService
{
    private const int DashboardTrendMonths = 6;
    private const int DashboardTrendDays = 7;
    private const int DashboardListSize = 5;
    private const int MaxActivityDays = 90;
    private const int MaxTrendMonths = 24;
    private const int OpenFraudWindowDays = 7;

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);
        var dayStart = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var steps = await reporting.GetStepTotalsAsync(today, cancellationToken);
        var points = await reporting.GetPointsTotalsAsync(dayStart, cancellationToken);
        var recent = await reporting.GetRecentLedgerAsync(DashboardListSize * 2, cancellationToken);
        var top = await reporting.GetTopUsersByStepsAsync(today, DashboardListSize, cancellationToken);

        return new AdminDashboardDto(
            await reporting.CountUsersAsync(cancellationToken),
            await reporting.CountUsersCreatedSinceAsync(monthStart, cancellationToken),
            steps.ActiveUsers,
            steps.Steps,
            steps.GoalsCompleted,
            points.IssuedToday,
            points.SpentToday,
            points.OutstandingBalance,
            await fraudSignals.CountSinceAsync(dayStart.AddDays(-OpenFraudWindowDays), cancellationToken),
            await BuildUserGrowthAsync(now, DashboardTrendMonths, cancellationToken),
            await BuildStepTrendAsync(today, DashboardTrendDays, cancellationToken),
            await BuildPointsFlowAsync(now, DashboardTrendMonths, cancellationToken),
            recent.Select(ToActivity).ToList(),
            top.Select((u, index) => new AdminTopUserDto(index + 1, u.UserId, u.Name, u.Steps, u.Points)).ToList());
    }

    public async Task<AdminActivityReportDto> GetActivityAsync(int days, CancellationToken cancellationToken = default)
    {
        var window = Math.Clamp(days, 1, MaxActivityDays);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var totals = await reporting.GetStepTotalsAsync(today, cancellationToken);
        var hourly = await reporting.GetSyncActivityByHourAsync(today, cancellationToken);
        var byHour = hourly.ToDictionary(h => h.Hour);

        return new AdminActivityReportDto(
            totals.Steps,
            totals.ActiveUsers,
            totals.ActiveUsers == 0 ? 0 : totals.Steps / totals.ActiveUsers,
            totals.GoalsCompleted,
            await BuildStepTrendAsync(today, window, cancellationToken),
            Enumerable.Range(0, 24)
                .Select(hour => byHour.TryGetValue(hour, out var row)
                    ? new HourBucketDto(hour, row.Syncs, row.Steps)
                    : new HourBucketDto(hour, 0, 0))
                .ToList());
    }

    public async Task<AdminPointsReportDto> GetPointsAsync(int months, CancellationToken cancellationToken = default)
    {
        var window = Math.Clamp(months, 1, MaxTrendMonths);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var dayStart = DateOnly.FromDateTime(now).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var totals = await reporting.GetPointsTotalsAsync(dayStart, cancellationToken);
        var rules = await settings.GetAsync(cancellationToken);

        return new AdminPointsReportDto(
            totals.IssuedToday,
            totals.SpentToday,
            totals.LifetimeEarned,
            totals.LifetimeSpent,
            totals.OutstandingBalance,
            await BuildPointsFlowAsync(now, window, cancellationToken),
            new PointsRulesDto(rules.DailyGoalReward, rules.AdReward, rules.StreakReward, rules.SpinMaxReward));
    }

    public async Task<IReadOnlyList<AdminFraudSignalDto>> GetFraudSignalsAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var rows = await reporting.GetRecentFraudSignalsAsync(Math.Clamp(take, 1, 200), cancellationToken);
        return rows
            .Select(r => new AdminFraudSignalDto(
                r.Id,
                r.UserId,
                r.UserName,
                r.SignalType,
                r.Severity,
                r.Description,
                r.DetectedAt))
            .ToList();
    }

    public async Task<IReadOnlyList<AdminAuditEntryDto>> GetAuditTrailAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var entries = await auditLog.ListRecentAsync(Math.Clamp(take, 1, 200), cancellationToken);
        return entries
            .Select(e => new AdminAuditEntryDto(
                e.Id,
                e.AdminUser?.DisplayName ?? e.AdminUser?.Email ?? "system",
                e.ActionType,
                e.EntityName,
                e.EntityId,
                e.OldValue,
                e.NewValue,
                e.IpAddress,
                e.CreatedAt))
            .ToList();
    }

    private async Task<IReadOnlyList<MonthValueDto>> BuildUserGrowthAsync(
        DateTime now,
        int months,
        CancellationToken cancellationToken)
    {
        var from = FirstOfMonth(now).AddMonths(-(months - 1));
        var rows = await reporting.GetSignupsByMonthAsync(from, cancellationToken);
        var byMonth = rows.ToDictionary(r => (r.Year, r.Month), r => (long)r.Count);

        // Running total, because "user growth" on the dashboard is cumulative rather than per-month signups.
        var baseline = await reporting.CountUsersAsync(cancellationToken) - rows.Sum(r => r.Count);

        var series = new List<MonthValueDto>(months);
        var running = (long)baseline;
        foreach (var month in MonthRange(from, months))
        {
            running += byMonth.GetValueOrDefault((month.Year, month.Month), 0);
            series.Add(new MonthValueDto(Label(month), month.Year, month.Month, running));
        }

        return series;
    }

    private async Task<IReadOnlyList<MonthFlowDto>> BuildPointsFlowAsync(
        DateTime now,
        int months,
        CancellationToken cancellationToken)
    {
        var from = FirstOfMonth(now).AddMonths(-(months - 1));
        var rows = await reporting.GetPointsByMonthAsync(from, cancellationToken);
        var byMonth = rows.ToDictionary(r => (r.Year, r.Month));

        return MonthRange(from, months)
            .Select(month => byMonth.TryGetValue((month.Year, month.Month), out var row)
                ? new MonthFlowDto(Label(month), month.Year, month.Month, row.Earned, row.Spent)
                : new MonthFlowDto(Label(month), month.Year, month.Month, 0, 0))
            .ToList();
    }

    private async Task<IReadOnlyList<DayStepsDto>> BuildStepTrendAsync(
        DateOnly today,
        int days,
        CancellationToken cancellationToken)
    {
        var from = today.AddDays(-(days - 1));
        var rows = await reporting.GetStepsByDayAsync(from, today, cancellationToken);
        var byDate = rows.ToDictionary(r => r.Date);

        return Enumerable.Range(0, days)
            .Select(offset => from.AddDays(offset))
            .Select(date => byDate.TryGetValue(date, out var row)
                ? new DayStepsDto(date, row.Steps, row.ActiveUsers)
                : new DayStepsDto(date, 0, 0))
            .ToList();
    }

    private static AdminActivityItemDto ToActivity(LedgerEntryRow row) => new(
        row.Id,
        row.UserName,
        row.TransactionType switch
        {
            PointsTransactionType.StepsEarned => "Completed daily goal",
            PointsTransactionType.AdEarned => "Watched a rewarded ad",
            PointsTransactionType.RedemptionDeducted => "Redeemed a reward",
            PointsTransactionType.TicketDeducted => "Entered a lucky draw",
            PointsTransactionType.AdminAdjustment => "Balance adjusted by an operator",
            PointsTransactionType.Expired => "Points expired",
            PointsTransactionType.Reversed => "Transaction reversed",
            _ => row.TransactionType.ToString(),
        },
        row.Description ?? $"{row.Amount:+#;-#;0} points",
        row.OccurredAt);

    private static IEnumerable<DateTime> MonthRange(DateTime from, int months) =>
        Enumerable.Range(0, months).Select(from.AddMonths);

    private static DateTime FirstOfMonth(DateTime value) =>
        new(value.Year, value.Month, 1, 0, 0, 0, DateTimeKind.Utc);

    private static string Label(DateTime month) => month.ToString("MMM yyyy", CultureInfo.InvariantCulture);
}
