using MOVOQ.Shared;

namespace MOVOQ.Application.Abstractions.Persistence;

/// <summary>
/// Read-only aggregate queries that back the admin dashboards. Kept apart from the write repositories
/// because every method here is a projection the transactional code paths never need.
/// </summary>
public interface IAdminReportingRepository
{
    Task<int> CountUsersAsync(CancellationToken cancellationToken = default);

    Task<int> CountUsersByStatusAsync(UserStatus status, CancellationToken cancellationToken = default);

    Task<int> CountUsersCreatedSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken = default);

    Task<StepsTotalsRow> GetStepTotalsAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StepsDayRow>> GetStepsByDayAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthCountRow>> GetSignupsByMonthAsync(DateTime fromUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonthPointsRow>> GetPointsByMonthAsync(DateTime fromUtc, CancellationToken cancellationToken = default);

    Task<PointsTotalsRow> GetPointsTotalsAsync(DateTime dayStartUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopUserRow>> GetTopUsersByStepsAsync(DateOnly date, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HourBucketRow>> GetSyncActivityByHourAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<AdminUserPage> SearchUsersAsync(
        string? query,
        UserStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AdminUserRow?> GetUserRowAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LedgerEntryRow>> GetRecentLedgerAsync(int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LedgerEntryRow>> GetUserLedgerAsync(Guid userId, int take, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FraudSignalRow>> GetRecentFraudSignalsAsync(int take, CancellationToken cancellationToken = default);
}
