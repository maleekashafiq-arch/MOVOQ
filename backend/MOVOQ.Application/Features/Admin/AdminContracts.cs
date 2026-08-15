using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

/// <summary>The resolved back-office operator behind the current request.</summary>
public sealed record AdminContext(Guid AdminUserId, string Email, string? DisplayName, AdminRole Role);

public sealed record AdminSessionDto(
    Guid AdminUserId,
    string Email,
    string? DisplayName,
    AdminRole Role,
    DateTime? LastLoginAt);

// ---------- shared chart shapes ----------

/// <summary>A month bucket. <paramref name="Label"/> is pre-formatted ("Mar 2026") so every client renders it the same.</summary>
public sealed record MonthValueDto(string Label, int Year, int Month, long Value);

public sealed record MonthFlowDto(string Label, int Year, int Month, long Earned, long Spent);

public sealed record DayStepsDto(DateOnly Date, long Steps, int ActiveUsers);

public sealed record HourBucketDto(int Hour, int Syncs, long Steps);

// ---------- dashboard ----------

public sealed record AdminActivityItemDto(Guid Id, string User, string Action, string Details, DateTime OccurredAt);

public sealed record AdminTopUserDto(int Rank, Guid UserId, string Name, long Steps, long Points);

public sealed record AdminDashboardDto(
    int TotalUsers,
    int NewUsersThisMonth,
    int ActiveUsersToday,
    long StepsToday,
    int GoalsCompletedToday,
    long PointsIssuedToday,
    long PointsSpentToday,
    long OutstandingBalance,
    int OpenFraudSignals,
    IReadOnlyList<MonthValueDto> UserGrowth,
    IReadOnlyList<DayStepsDto> StepActivity,
    IReadOnlyList<MonthFlowDto> PointsFlow,
    IReadOnlyList<AdminActivityItemDto> RecentActivity,
    IReadOnlyList<AdminTopUserDto> TopUsers);

// ---------- activity ----------

public sealed record AdminActivityReportDto(
    long StepsToday,
    int ActiveUsersToday,
    long AverageStepsPerActiveUser,
    int GoalsCompletedToday,
    IReadOnlyList<DayStepsDto> Daily,
    IReadOnlyList<HourBucketDto> SyncsByHour);

// ---------- points ----------

public sealed record PointsRulesDto(int DailyGoalReward, int AdReward, int StreakReward, int SpinMaxReward);

public sealed record AdminPointsReportDto(
    long IssuedToday,
    long SpentToday,
    long LifetimeEarned,
    long LifetimeSpent,
    long OutstandingBalance,
    IReadOnlyList<MonthFlowDto> Flow,
    PointsRulesDto Rules);

// ---------- users ----------

public sealed record AdminUserDto(
    Guid Id,
    string Email,
    string Name,
    string? PhoneNumber,
    string? City,
    string? Country,
    string? Gender,
    int DailyTargetSteps,
    long Points,
    long Steps,
    int Tickets,
    UserStatus Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public sealed record AdminUserListDto(
    IReadOnlyList<AdminUserDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int ActiveCount,
    int SuspendedCount,
    int PendingCount);

public sealed record AdminUserStepDayDto(DateOnly Date, int Steps, int Goal, bool GoalReached, long PointsAwarded);

public sealed record AdminLedgerEntryDto(
    Guid Id,
    long Amount,
    PointsTransactionType TransactionType,
    string? Description,
    DateTime OccurredAt);

public sealed record AdminUserDetailDto(
    AdminUserDto User,
    IReadOnlyList<AdminUserStepDayDto> StepHistory,
    IReadOnlyList<AdminLedgerEntryDto> RecentLedger);

public sealed record AdjustPointsCommand(long Amount, string Reason);

public sealed record ChangeUserStatusCommand(UserStatus Status, string? Reason);

public sealed record AdjustPointsResultDto(Guid UserId, long Amount, long Balance);

// ---------- fraud ----------

public sealed record AdminFraudSignalDto(
    Guid Id,
    Guid UserId,
    string UserName,
    FraudSignalType SignalType,
    FraudSignalSeverity Severity,
    string Description,
    DateTime DetectedAt);

// ---------- audit ----------

public sealed record AdminAuditEntryDto(
    Guid Id,
    string Operator,
    AuditActionType Action,
    string EntityName,
    Guid? EntityId,
    string? OldValue,
    string? NewValue,
    string? IpAddress,
    DateTime OccurredAt);

// ---------- settings ----------

public sealed record PlatformSettingsDto(
    string AppName,
    string SupportEmail,
    int DailyGoalReward,
    int AdReward,
    int StreakReward,
    int SpinMaxReward,
    int MaxPointsPerDay,
    int MaxTicketsPerDay,
    int MaxAdsPerDay,
    int TicketConversionRate,
    int TicketExpiryDays,
    int AdFrequency,
    bool AdsOnHome,
    bool AdsOnMarketplace,
    int CoachPrice,
    int CoachTrialDays,
    bool FeatureNotifications,
    bool FeatureAds,
    bool FeatureLuckyDraws,
    bool FeatureTeams,
    bool FeatureAiCoach);
