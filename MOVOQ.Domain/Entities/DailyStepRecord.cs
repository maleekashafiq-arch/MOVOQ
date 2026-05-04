using MOVOQ.Domain.Abstractions;
using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Daily step sync from mobile. Client claim fields are untrusted input; server verdict fields drive eligibility and ledger posting.</summary>
public sealed class DailyStepRecord : BaseEntity, ISyncableEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>Calendar day the steps belong to (device-local date semantics are normalized in Application later).</summary>
    public DateOnly StepDate { get; set; }

    // --- Client / mobile claims (untrusted; do not post points from these alone) ---

    public int ClaimedStepCount { get; set; }

    /// <summary>Device-reported clock for the step day/event (metadata; server rules use receipt/validation time).</summary>
    public DateTime? DeviceRecordedAt { get; set; }

    public Guid? UserDeviceId { get; set; }

    public UserDevice? UserDevice { get; set; }

    // --- Backend / server verdict (authoritative for points and ledger links) ---

    public int? ValidatedStepCount { get; set; }

    public int? DailyTargetStepsSnapshot { get; set; }

    public bool TargetCompleted { get; set; }

    public bool PointsEligible { get; set; }

    public bool PointsAwarded { get; set; }

    public long PointsAmount { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public ValidationStatus ValidationStatus { get; set; } = ValidationStatus.Pending;

    public Guid? PointsLedgerId { get; set; }

    public PointsLedger? PointsLedger { get; set; }

    // --- Sync envelope (ISyncableEntity: CommandId + DeviceId identify replay; Source/SyncedAt are sync metadata) ---

    public Guid CommandId { get; set; }

    public Guid? DeviceId { get; set; }

    public DataSource Source { get; set; } = DataSource.Mobile;

    public DateTime? SyncedAt { get; set; }
}
