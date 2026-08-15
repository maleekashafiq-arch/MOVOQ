using MOVOQ.Domain.Abstractions;
using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Ad completion reported by mobile or ad network. Rewarded points require a backend-issued <see cref="AdSessionId"/>.</summary>
/// <remarks>
/// Invariant (must be enforced in Application / domain factories): when <see cref="IsRewarded"/> is false,
/// <see cref="PointsAmount"/> must be 0 and <see cref="PointsAwarded"/> must be false (forced ads never grant points).
/// </remarks>
public sealed class AdsWatched : BaseEntity, ISyncableEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    // --- Client / ad-network claims (untrusted; do not post points from these alone) ---

    public Guid AdSessionId { get; set; }

    public AdType AdType { get; set; }

    public string? AdProvider { get; set; }

    public string? ProviderTransactionId { get; set; }

    /// <summary>Device- or SDK-reported completion time (metadata).</summary>
    public DateTime DeviceWatchedAt { get; set; }

    public Guid? UserDeviceId { get; set; }

    public UserDevice? UserDevice { get; set; }

    // --- Backend / server verdict (authoritative for rewards and ledger links) ---

    public bool IsRewarded { get; set; }

    /// <summary>Server approval that this completion counts as a rewarded completion (distinct from ad network &quot;rewarded&quot; wording).</summary>
    public bool RewardApproved { get; set; }

    public bool PointsAwarded { get; set; }

    public long PointsAmount { get; set; }

    public DateTime? ValidatedAt { get; set; }

    public ValidationStatus ValidationStatus { get; set; } = ValidationStatus.Pending;

    public Guid? PointsLedgerId { get; set; }

    public PointsLedger? PointsLedger { get; set; }

    // --- Sync envelope (ISyncableEntity) ---

    public Guid CommandId { get; set; }

    public Guid? DeviceId { get; set; }

    public DataSource Source { get; set; } = DataSource.Mobile;

    public DateTime? SyncedAt { get; set; }
}
