using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Tests.Unit.Admin.Fakes;

internal sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }

    public Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default) =>
        action(cancellationToken);
}

internal sealed class FakeCallerContext(string? ipAddress) : ICallerContext
{
    public string? IpAddress { get; } = ipAddress;
}

internal sealed class FakeAdminUserRepository : IAdminUserRepository
{
    public List<AdminUser> Items { get; } = [];

    public void Add(AdminUser admin) => Items.Add(admin);

    public Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

    public Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Email == email));

    public Task<IReadOnlyList<AdminUser>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AdminUser>>(Items);
}

internal sealed class FakeAuditLogRepository : IAuditLogRepository
{
    public List<AuditLog> Items { get; } = [];

    public void Add(AuditLog entry) => Items.Add(entry);

    public Task<IReadOnlyList<AuditLog>> ListRecentAsync(int take, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AuditLog>>(Items
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToList());
}

internal sealed class FakeAppSettingRepository : IAppSettingRepository
{
    public List<AppSetting> Items { get; } = [];

    public void Add(AppSetting setting) => Items.Add(setting);

    public Task<IReadOnlyList<AppSetting>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AppSetting>>(Items);
}

internal sealed class FakeUserRepository : IUserRepository
{
    public List<User> Items { get; } = [];

    public void Add(User user) => Items.Add(user);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Email == email));

    public Task<User?> GetByIdForIdentityAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<User>> ListForDevAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<User>>(Items);
}

internal sealed class FakeWalletRepository : IUserWalletRepository
{
    public List<UserWallet> Items { get; } = [];

    public void Add(UserWallet wallet) => Items.Add(wallet);

    public Task LockByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

internal sealed class FakeLedgerRepository : IPointsLedgerRepository
{
    public List<PointsLedger> Items { get; } = [];

    public void Add(PointsLedger entry) => Items.Add(entry);

    public Task<long> SumLifetimeAmountsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Where(x => x.UserId == userId).Sum(x => x.Amount));
}

internal sealed class FakeStepRecordRepository : IDailyStepRecordRepository
{
    public List<DailyStepRecord> Items { get; } = [];

    public void Add(DailyStepRecord record) => Items.Add(record);

    public Task<DailyStepRecord?> GetByUserAndDateAsync(
        Guid userId,
        DateOnly stepDate,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.UserId == userId && x.StepDate == stepDate));

    public Task<IReadOnlyList<DailyStepRecord>> ListRangeAsync(
        Guid userId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<DailyStepRecord>>(Items
            .Where(x => x.UserId == userId && x.StepDate >= fromDate && x.StepDate <= toDate)
            .OrderBy(x => x.StepDate)
            .ToList());
}

/// <summary>
/// Serves admin projections straight from an in-memory user list, so service tests exercise the mapping and
/// paging rules without a database.
/// </summary>
internal sealed class FakeAdminReportingRepository(IReadOnlyList<User> users) : IAdminReportingRepository
{
    public List<LedgerEntryRow> Ledger { get; } = [];

    public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(users.Count);

    public Task<int> CountUsersByStatusAsync(UserStatus status, CancellationToken cancellationToken = default) =>
        Task.FromResult(users.Count(x => x.Status == status));

    public Task<int> CountUsersCreatedSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken = default) =>
        Task.FromResult(users.Count(x => x.CreatedAt >= sinceUtc));

    public Task<AdminUserPage> SearchUsersAsync(
        string? query,
        UserStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var matches = users.AsEnumerable();

        if (status is not null)
        {
            matches = matches.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            matches = matches.Where(x =>
                x.Email.Contains(query, StringComparison.OrdinalIgnoreCase)
                || (x.Profile?.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var all = matches.ToList();
        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToRow)
            .ToList();

        return Task.FromResult(new AdminUserPage(items, all.Count));
    }

    public Task<AdminUserRow?> GetUserRowAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = users.SingleOrDefault(x => x.Id == userId);
        return Task.FromResult(user is null ? null : ToRow(user));
    }

    public Task<IReadOnlyList<LedgerEntryRow>> GetUserLedgerAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<LedgerEntryRow>>(Ledger.Where(x => x.UserId == userId).Take(take).ToList());

    public Task<IReadOnlyList<LedgerEntryRow>> GetRecentLedgerAsync(
        int take,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<LedgerEntryRow>>(Ledger.Take(take).ToList());

    public Task<StepsTotalsRow> GetStepTotalsAsync(DateOnly date, CancellationToken cancellationToken = default) =>
        Task.FromResult(new StepsTotalsRow(0, 0, 0));

    public Task<IReadOnlyList<StepsDayRow>> GetStepsByDayAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<StepsDayRow>>([]);

    public Task<IReadOnlyList<MonthCountRow>> GetSignupsByMonthAsync(
        DateTime fromUtc,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<MonthCountRow>>([]);

    public Task<IReadOnlyList<MonthPointsRow>> GetPointsByMonthAsync(
        DateTime fromUtc,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<MonthPointsRow>>([]);

    public Task<PointsTotalsRow> GetPointsTotalsAsync(
        DateTime dayStartUtc,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new PointsTotalsRow(0, 0, 0, 0, 0));

    public Task<IReadOnlyList<TopUserRow>> GetTopUsersByStepsAsync(
        DateOnly date,
        int take,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TopUserRow>>([]);

    public Task<IReadOnlyList<HourBucketRow>> GetSyncActivityByHourAsync(
        DateOnly date,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<HourBucketRow>>([]);

    public Task<IReadOnlyList<FraudSignalRow>> GetRecentFraudSignalsAsync(
        int take,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<FraudSignalRow>>([]);

    private static AdminUserRow ToRow(User user) => new(
        user.Id,
        user.Email,
        user.Profile?.DisplayName,
        user.PhoneNumber,
        user.Profile?.City,
        user.Profile?.Country,
        user.Profile?.Gender,
        user.StepGoals.FirstOrDefault(g => g.IsActive)?.DailyTargetSteps ?? DailyGoalOptions.Default,
        user.Wallet?.Balance ?? 0,
        0,
        0,
        user.Status,
        user.CreatedAt,
        user.LastLoginAt);
}

internal sealed class FakeLuckyDrawRepository : ILuckyDrawRepository
{
    public List<LuckyDraw> Items { get; } = [];

    public void Add(LuckyDraw draw) => Items.Add(draw);

    public Task<LuckyDraw?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

    public Task<LuckyDraw?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetAsync(id, cancellationToken);

    public Task<IReadOnlyList<LuckyDraw>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<LuckyDraw>>(Items.OrderByDescending(x => x.CreatedAt).ToList());

    public Task<IReadOnlyList<LuckyDraw>> ListOpenAsync(DateTime now, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<LuckyDraw>>(Items
            .Where(x => x.Status == LuckyDrawStatus.Active && x.StartAt <= now && x.EndAt > now)
            .ToList());
}

internal sealed class FakeLuckyDrawTicketRepository : ILuckyDrawTicketRepository
{
    public List<LuckyDrawTicket> Items { get; } = [];

    public Dictionary<Guid, string> UserNames { get; } = [];

    public Dictionary<Guid, string> DrawNames { get; } = [];

    public void Add(LuckyDrawTicket ticket) => Items.Add(ticket);

    public Task<IReadOnlyList<DrawTicketTotals>> GetTotalsAsync(
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<DrawTicketTotals>>(drawIds
            .Select(id =>
            {
                var rows = Items.Where(x => x.LuckyDrawId == id && x.Status != TicketStatus.Cancelled).ToList();
                return new DrawTicketTotals(
                    id,
                    rows.Count,
                    rows.Select(x => x.UserId).Distinct().Count(),
                    rows.Sum(x => x.PointsCostSnapshot));
            })
            .ToList());

    public Task<IReadOnlyDictionary<Guid, int>> CountForUserAsync(
        Guid userId,
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyDictionary<Guid, int>>(Items
            .Where(x => x.UserId == userId && drawIds.Contains(x.LuckyDrawId) && x.Status != TicketStatus.Cancelled)
            .GroupBy(x => x.LuckyDrawId)
            .ToDictionary(g => g.Key, g => g.Count()));

    public Task<int> CountForUserSinceAsync(Guid userId, DateTime since, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Count(x =>
            x.UserId == userId && x.PurchasedAt >= since && x.Status != TicketStatus.Cancelled));

    public Task<IReadOnlyList<LuckyDrawTicket>> ListActiveForDrawAsync(
        Guid drawId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<LuckyDrawTicket>>(Items
            .Where(x => x.LuckyDrawId == drawId && x.Status == TicketStatus.Active)
            .ToList());

    public Task<IReadOnlyList<TicketRow>> ListForDrawAsync(
        Guid drawId,
        int take,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TicketRow>>(Rows(Items.Where(x => x.LuckyDrawId == drawId), take));

    public Task<IReadOnlyList<TicketRow>> ListRecentAsync(int take, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TicketRow>>(Rows(Items, take));

    public Task<TicketDayTotals> GetTotalsSinceAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        var rows = Items.Where(x => x.PurchasedAt >= since && x.Status != TicketStatus.Cancelled).ToList();
        return Task.FromResult(new TicketDayTotals(
            rows.Count,
            rows.Select(x => x.UserId).Distinct().Count(),
            rows.Sum(x => x.PointsCostSnapshot)));
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(Items.Count);

    public Task<int> CountByStatusAsync(TicketStatus status, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Count(x => x.Status == status));

    private List<TicketRow> Rows(IEnumerable<LuckyDrawTicket> source, int take) =>
        source
            .OrderByDescending(x => x.PurchasedAt)
            .Take(take)
            .Select(x => new TicketRow(
                x.Id,
                x.TicketNumber,
                x.UserId,
                UserNames.GetValueOrDefault(x.UserId, "Walker"),
                x.LuckyDrawId,
                DrawNames.GetValueOrDefault(x.LuckyDrawId, "Draw"),
                x.PointsCostSnapshot,
                x.Status,
                x.PurchasedAt))
            .ToList();
}

internal sealed class FakeLuckyDrawWinnerRepository : ILuckyDrawWinnerRepository
{
    public List<LuckyDrawWinner> Items { get; } = [];

    public Dictionary<Guid, string> UserNames { get; } = [];

    public void Add(LuckyDrawWinner winner) => Items.Add(winner);

    public Task<LuckyDrawWinner?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<WinnerRow>> ListForDrawsAsync(
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<WinnerRow>>(Items
            .Where(x => drawIds.Contains(x.LuckyDrawId))
            .Select(ToRow)
            .ToList());

    public Task<IReadOnlyList<WinnerRow>> ListRecentAsync(int take, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<WinnerRow>>(Items
            .OrderByDescending(x => x.SelectedAt)
            .Take(take)
            .Select(ToRow)
            .ToList());

    public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(Items.Count);

    public Task<int> CountByFulfillmentAsync(
        PrizeFulfillmentStatus status,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Count(x => x.FulfillmentStatus == status));

    private WinnerRow ToRow(LuckyDrawWinner winner) => new(
        winner.Id,
        winner.LuckyDrawId,
        winner.LuckyDraw?.Name ?? "Draw",
        winner.LuckyDraw?.PrizeDescription ?? "Prize",
        winner.LuckyDrawTicketId,
        winner.LuckyDrawTicket?.TicketNumber ?? "T-0",
        winner.UserId,
        UserNames.GetValueOrDefault(winner.UserId, "Walker"),
        winner.SelectedAt,
        winner.SelectionMethod,
        winner.FulfillmentStatus,
        winner.Notes);
}

internal sealed class FakeSponsorRepository : ISponsorRepository
{
    public List<Sponsor> Items { get; } = [];

    public void Add(Sponsor sponsor) => Items.Add(sponsor);

    public Task<Sponsor?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

    public Task<IReadOnlyList<Sponsor>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Sponsor>>(Items.OrderBy(x => x.Name).ToList());
}

internal sealed class FakeRewardRepository : IRewardRepository
{
    public List<Reward> Items { get; } = [];

    public void Add(Reward reward) => Items.Add(reward);

    public Task<Reward?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

    public Task<Reward?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetAsync(id, cancellationToken);

    public Task<IReadOnlyList<Reward>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Reward>>(Items.OrderBy(x => x.Name).ToList());
}

internal sealed class FakeRewardRedemptionRepository : IRewardRedemptionRepository
{
    public List<RewardRedemption> Items { get; } = [];

    public void Add(RewardRedemption redemption) => Items.Add(redemption);

    public Task<IReadOnlyList<RewardRedemption>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RewardRedemption>>(Items
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.RequestedAt)
            .ToList());

    public Task<IReadOnlyList<RedemptionCount>> CountByRewardAsync(
        IReadOnlyCollection<Guid> rewardIds,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RedemptionCount>>(rewardIds
            .Select(id => new RedemptionCount(id, Items.Count(x => x.RewardId == id && x.Status != RedemptionStatus.Cancelled)))
            .ToList());

    public Task<IReadOnlyList<RedemptionCount>> CountBySponsorAsync(
        IReadOnlyCollection<Guid> sponsorIds,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RedemptionCount>>(sponsorIds
            .Select(id => new RedemptionCount(
                id,
                Items.Count(x => x.Reward.SponsorId == id && x.Status != RedemptionStatus.Cancelled)))
            .ToList());

    public Task<int> CountSinceAsync(DateTime since, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Count(x => x.RequestedAt >= since && x.Status != RedemptionStatus.Cancelled));

    public Task<long> PointsRedeemedSinceAsync(DateTime since, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items
            .Where(x => x.RequestedAt >= since && x.Status == RedemptionStatus.Confirmed)
            .Sum(x => x.PointsCostSnapshot));
}
