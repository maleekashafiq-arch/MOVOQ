using MOVOQ.Domain.Abstractions;
using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Daily step payload from mobile. Points are decided by the server using goals, caps, and ad recovery rules.</summary>
public sealed class DailyStepRecord : BaseEntity, ISyncableEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>Calendar day the steps belong to (device-local date semantics are normalized in Application later).</summary>
    public DateOnly StepDate { get; set; }

    public int StepCount { get; set; }

    public int? DailyTargetStepsSnapshot { get; set; }

    public bool TargetCompleted { get; set; }

    public bool PointsEligible { get; set; }

    public bool PointsAwarded { get; set; }

    public Guid? UserDeviceId { get; set; }

    public UserDevice? UserDevice { get; set; }

    public Guid CommandId { get; set; }

    public Guid? DeviceId { get; set; }

    public DataSource Source { get; set; } = DataSource.Mobile;

    public DateTime? SyncedAt { get; set; }
}
