using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Steps;

/// <summary>A device's claim for one calendar day. Everything here is untrusted until the server validates it.</summary>
public sealed record SyncStepsCommand(
    Guid CommandId,
    DateOnly StepDate,
    int StepCount,
    string? DeviceIdentifier,
    DateTime? DeviceRecordedAt);

/// <summary>The server's verdict for one day.</summary>
public sealed record DailyStepsDto(
    DateOnly StepDate,
    int Steps,
    int DailyTargetSteps,
    bool GoalCompleted,
    long PointsAwarded,
    ValidationStatus ValidationStatus);

/// <summary>Everything Home needs after a sync, so the screen can repaint from a single response.</summary>
public sealed record StepsSyncResultDto(
    DailyStepsDto Day,
    long PointsCredited,
    long WalletBalance,
    int CurrentStreak,
    bool Duplicate);

public sealed record StepsHistoryDto(
    IReadOnlyList<DailyStepsDto> Days,
    int TotalSteps,
    long TotalPointsAwarded,
    int CurrentStreak);
