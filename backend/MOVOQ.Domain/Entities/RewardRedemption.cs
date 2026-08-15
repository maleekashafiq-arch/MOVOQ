using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Reward redemption workflow; points movement is traceable via <see cref="PointsLedgerId"/>.</summary>
public sealed class RewardRedemption : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid RewardId { get; set; }

    public Reward Reward { get; set; } = null!;

    public RedemptionStatus Status { get; set; } = RedemptionStatus.Pending;

    public long PointsCostSnapshot { get; set; }

    public Guid? PointsLedgerId { get; set; }

    public PointsLedger? PointsLedger { get; set; }

    public string? AssignedCode { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public string? FailureReason { get; set; }
}
