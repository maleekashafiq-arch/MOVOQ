using MOVOQ.Shared;

namespace MOVOQ.Application.Abstractions.Persistence;

/// <summary>
/// Flattened read models for back-office reporting. These are projections rather than entities: admin
/// screens need cross-table aggregates that would be far too expensive to assemble by loading graphs.
/// </summary>
public sealed record AdminUserRow(
    Guid Id,
    string Email,
    string? DisplayName,
    string? PhoneNumber,
    string? City,
    string? Country,
    string? Gender,
    int DailyTargetSteps,
    long PointsBalance,
    long LifetimeSteps,
    int ActiveTickets,
    UserStatus Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public sealed record AdminUserPage(IReadOnlyList<AdminUserRow> Items, int TotalCount);

/// <summary>One calendar day of platform-wide step activity.</summary>
public sealed record StepsDayRow(DateOnly Date, long Steps, int ActiveUsers);

public sealed record StepsTotalsRow(long Steps, int ActiveUsers, int GoalsCompleted);

public sealed record MonthCountRow(int Year, int Month, int Count);

public sealed record MonthPointsRow(int Year, int Month, long Earned, long Spent);

/// <summary>Sync traffic bucketed by UTC hour. Devices report a day at a time, so this measures when
/// people sync rather than when they walked.</summary>
public sealed record HourBucketRow(int Hour, int Syncs, long Steps);

public sealed record TopUserRow(Guid UserId, string Name, long Steps, long Points);

public sealed record PointsTotalsRow(
    long IssuedToday,
    long SpentToday,
    long LifetimeEarned,
    long LifetimeSpent,
    long OutstandingBalance);

public sealed record LedgerEntryRow(
    Guid Id,
    Guid UserId,
    string UserName,
    long Amount,
    PointsTransactionType TransactionType,
    string? Description,
    DateTime OccurredAt);

public sealed record FraudSignalRow(
    Guid Id,
    Guid UserId,
    string UserName,
    FraudSignalType SignalType,
    FraudSignalSeverity Severity,
    string Description,
    DateTime DetectedAt);
