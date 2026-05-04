using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Marketplace reward, coupon, or sponsor deal.</summary>
public sealed class Reward : BaseEntity
{
    public Guid SponsorId { get; set; }

    public Sponsor Sponsor { get; set; } = null!;

    public required string Name { get; set; }

    public string? Description { get; set; }

    public int PointsCost { get; set; }

    public int? StockQuantity { get; set; }

    public RewardStatus Status { get; set; } = RewardStatus.Draft;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidUntil { get; set; }

    public string? TermsAndConditions { get; set; }

    public ICollection<RewardRedemption> Redemptions { get; } = new List<RewardRedemption>();
}
