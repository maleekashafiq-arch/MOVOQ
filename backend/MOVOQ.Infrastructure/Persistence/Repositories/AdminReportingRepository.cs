using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

/// <summary>
/// Aggregate queries for the back office. Everything here is read-only and untracked; the projections stay
/// close to SQL so a dashboard never pulls entity graphs it only wants a count from.
/// </summary>
internal sealed class AdminReportingRepository(MovoqDbContext db) : IAdminReportingRepository
{
    public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) =>
        db.Users.CountAsync(cancellationToken);

    public Task<int> CountUsersByStatusAsync(UserStatus status, CancellationToken cancellationToken = default) =>
        db.Users.CountAsync(x => x.Status == status, cancellationToken);

    public Task<int> CountUsersCreatedSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken = default) =>
        db.Users.CountAsync(x => x.CreatedAt >= sinceUtc, cancellationToken);

    public async Task<StepsTotalsRow> GetStepTotalsAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var day = db.DailyStepRecords.AsNoTracking().Where(x => x.StepDate == date);

        // One row per user per day is enforced by a unique index, so a row count is a user count.
        var totals = await day
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Steps = g.Sum(x => (long)(x.ValidatedStepCount ?? x.ClaimedStepCount)),
                Active = g.Count(x => (x.ValidatedStepCount ?? x.ClaimedStepCount) > 0),
                Goals = g.Count(x => x.TargetCompleted),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return totals is null
            ? new StepsTotalsRow(0, 0, 0)
            : new StepsTotalsRow(totals.Steps, totals.Active, totals.Goals);
    }

    public async Task<IReadOnlyList<StepsDayRow>> GetStepsByDayAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.DailyStepRecords
            .AsNoTracking()
            .Where(x => x.StepDate >= from && x.StepDate <= to)
            .GroupBy(x => x.StepDate)
            .Select(g => new
            {
                Date = g.Key,
                Steps = g.Sum(x => (long)(x.ValidatedStepCount ?? x.ClaimedStepCount)),
                Active = g.Count(x => (x.ValidatedStepCount ?? x.ClaimedStepCount) > 0),
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new StepsDayRow(r.Date, r.Steps, r.Active)).ToList();
    }

    public async Task<IReadOnlyList<MonthCountRow>> GetSignupsByMonthAsync(
        DateTime fromUtc,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.Users
            .AsNoTracking()
            .Where(x => x.CreatedAt >= fromUtc)
            .GroupBy(x => new { x.CreatedAt.Year, x.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new MonthCountRow(r.Year, r.Month, r.Count)).ToList();
    }

    public async Task<IReadOnlyList<MonthPointsRow>> GetPointsByMonthAsync(
        DateTime fromUtc,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.PointsLedgers
            .AsNoTracking()
            .Where(x => x.OccurredAt >= fromUtc)
            .GroupBy(x => new { x.OccurredAt.Year, x.OccurredAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Earned = g.Sum(x => x.Amount > 0 ? x.Amount : 0),
                Spent = g.Sum(x => x.Amount < 0 ? -x.Amount : 0),
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new MonthPointsRow(r.Year, r.Month, r.Earned, r.Spent)).ToList();
    }

    public async Task<PointsTotalsRow> GetPointsTotalsAsync(
        DateTime dayStartUtc,
        CancellationToken cancellationToken = default)
    {
        var today = db.PointsLedgers.AsNoTracking().Where(x => x.OccurredAt >= dayStartUtc);
        var issued = await today.Where(x => x.Amount > 0).SumAsync(x => (long?)x.Amount, cancellationToken) ?? 0;
        var spent = await today.Where(x => x.Amount < 0).SumAsync(x => (long?)x.Amount, cancellationToken) ?? 0;

        var wallets = await db.UserWallets
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Earned = g.Sum(x => x.LifetimeEarnedPoints),
                Spent = g.Sum(x => x.LifetimeSpentPoints),
                Balance = g.Sum(x => x.Balance),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new PointsTotalsRow(
            issued,
            -spent,
            wallets?.Earned ?? 0,
            wallets?.Spent ?? 0,
            wallets?.Balance ?? 0);
    }

    public async Task<IReadOnlyList<TopUserRow>> GetTopUsersByStepsAsync(
        DateOnly date,
        int take,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.DailyStepRecords
            .AsNoTracking()
            .Where(x => x.StepDate == date)
            .OrderByDescending(x => x.ValidatedStepCount ?? x.ClaimedStepCount)
            .Take(take)
            .Select(x => new
            {
                x.UserId,
                DisplayName = x.User.Profile != null ? x.User.Profile.DisplayName : null,
                x.User.Email,
                Steps = (long)(x.ValidatedStepCount ?? x.ClaimedStepCount),
                Points = x.User.Wallet != null ? x.User.Wallet.Balance : 0,
            })
            .ToListAsync(cancellationToken);

        if (rows.Count > 0)
        {
            return rows
                .Select(r => new TopUserRow(r.UserId, DisplayName(r.DisplayName, r.Email), r.Steps, r.Points))
                .ToList();
        }

        // Nobody has synced yet today. Falling back to balances keeps the leaderboard useful overnight
        // and on a freshly seeded database.
        var byBalance = await db.Users
            .AsNoTracking()
            .OrderByDescending(x => x.Wallet != null ? x.Wallet.Balance : 0)
            .Take(take)
            .Select(x => new
            {
                UserId = x.Id,
                DisplayName = x.Profile != null ? x.Profile.DisplayName : null,
                x.Email,
                Points = x.Wallet != null ? x.Wallet.Balance : 0,
            })
            .ToListAsync(cancellationToken);

        return byBalance
            .Select(r => new TopUserRow(r.UserId, DisplayName(r.DisplayName, r.Email), 0, r.Points))
            .ToList();
    }

    public async Task<IReadOnlyList<HourBucketRow>> GetSyncActivityByHourAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.DailyStepRecords
            .AsNoTracking()
            .Where(x => x.StepDate == date && x.SyncedAt != null)
            .GroupBy(x => x.SyncedAt!.Value.Hour)
            .Select(g => new
            {
                Hour = g.Key,
                Syncs = g.Count(),
                Steps = g.Sum(x => (long)(x.ValidatedStepCount ?? x.ClaimedStepCount)),
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new HourBucketRow(r.Hour, r.Syncs, r.Steps)).ToList();
    }

    public async Task<AdminUserPage> SearchUsersAsync(
        string? query,
        UserStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var users = db.Users.AsNoTracking();

        if (status is not null)
        {
            users = users.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            users = users.Where(x =>
                EF.Functions.ILike(x.Email, pattern)
                || (x.PhoneNumber != null && EF.Functions.ILike(x.PhoneNumber, pattern))
                || (x.Profile != null && x.Profile.DisplayName != null && EF.Functions.ILike(x.Profile.DisplayName, pattern))
                || (x.Profile != null && x.Profile.City != null && EF.Functions.ILike(x.Profile.City, pattern)));
        }

        var total = await users.CountAsync(cancellationToken);
        var rows = await Project(users.OrderByDescending(x => x.CreatedAt))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new AdminUserPage(rows.Select(ToRow).ToList(), total);
    }

    public async Task<AdminUserRow?> GetUserRowAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var row = await Project(db.Users.AsNoTracking().Where(x => x.Id == userId))
            .SingleOrDefaultAsync(cancellationToken);

        return row is null ? null : ToRow(row);
    }

    public async Task<IReadOnlyList<LedgerEntryRow>> GetRecentLedgerAsync(
        int take,
        CancellationToken cancellationToken = default) =>
        await ReadLedgerAsync(db.PointsLedgers.AsNoTracking(), take, cancellationToken);

    public async Task<IReadOnlyList<LedgerEntryRow>> GetUserLedgerAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default) =>
        await ReadLedgerAsync(db.PointsLedgers.AsNoTracking().Where(x => x.UserId == userId), take, cancellationToken);

    public async Task<IReadOnlyList<FraudSignalRow>> GetRecentFraudSignalsAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var rows = await db.FraudSignals
            .AsNoTracking()
            .OrderByDescending(x => x.DetectedAt)
            .Take(take)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                DisplayName = x.User.Profile != null ? x.User.Profile.DisplayName : null,
                x.User.Email,
                x.SignalType,
                x.Severity,
                x.Description,
                x.DetectedAt,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new FraudSignalRow(
                r.Id,
                r.UserId,
                DisplayName(r.DisplayName, r.Email),
                r.SignalType,
                r.Severity,
                r.Description,
                r.DetectedAt))
            .ToList();
    }

    private static async Task<IReadOnlyList<LedgerEntryRow>> ReadLedgerAsync(
        IQueryable<PointsLedger> source,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await source
            .OrderByDescending(x => x.OccurredAt)
            .Take(take)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                DisplayName = x.User.Profile != null ? x.User.Profile.DisplayName : null,
                x.User.Email,
                x.Amount,
                x.TransactionType,
                x.Description,
                x.OccurredAt,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new LedgerEntryRow(
                r.Id,
                r.UserId,
                DisplayName(r.DisplayName, r.Email),
                r.Amount,
                r.TransactionType,
                r.Description,
                r.OccurredAt))
            .ToList();
    }

    private static IQueryable<UserProjection> Project(IQueryable<User> users) =>
        users.Select(x => new UserProjection(
            x.Id,
            x.Email,
            x.Profile != null ? x.Profile.DisplayName : null,
            x.PhoneNumber,
            x.Profile != null ? x.Profile.City : null,
            x.Profile != null ? x.Profile.Country : null,
            x.Profile != null ? x.Profile.Gender : null,
            x.StepGoals.Where(g => g.IsActive).Select(g => (int?)g.DailyTargetSteps).FirstOrDefault(),
            x.Wallet != null ? x.Wallet.Balance : 0,
            x.DailyStepRecords.Sum(r => (long)(r.ValidatedStepCount ?? r.ClaimedStepCount)),
            x.LuckyDrawTickets.Count(t => t.Status == TicketStatus.Active),
            x.Status,
            x.CreatedAt,
            x.LastLoginAt));

    private static AdminUserRow ToRow(UserProjection p) => new(
        p.Id,
        p.Email,
        p.DisplayName,
        p.PhoneNumber,
        p.City,
        p.Country,
        p.Gender,
        p.DailyTargetSteps ?? DailyGoalOptions.Default,
        p.PointsBalance,
        p.LifetimeSteps,
        p.ActiveTickets,
        p.Status,
        p.CreatedAt,
        p.LastLoginAt);

    /// <summary>Accounts that never completed a profile still need a label in admin tables.</summary>
    private static string DisplayName(string? displayName, string email)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        var at = email.IndexOf('@');
        return at > 0 ? email[..at] : email;
    }

    private sealed record UserProjection(
        Guid Id,
        string Email,
        string? DisplayName,
        string? PhoneNumber,
        string? City,
        string? Country,
        string? Gender,
        int? DailyTargetSteps,
        long PointsBalance,
        long LifetimeSteps,
        int ActiveTickets,
        UserStatus Status,
        DateTime CreatedAt,
        DateTime? LastLoginAt);
}
