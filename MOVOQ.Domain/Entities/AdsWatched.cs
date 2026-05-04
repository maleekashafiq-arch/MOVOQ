using MOVOQ.Domain.Abstractions;
using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Ad completion reported by mobile. Rewarded points require a backend-issued <see cref="AdSessionId"/>.</summary>
public sealed class AdsWatched : BaseEntity, ISyncableEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid AdSessionId { get; set; }

    public AdType AdType { get; set; }

    public string? AdProvider { get; set; }

    public string? ProviderTransactionId { get; set; }

    public DateTime CompletedAt { get; set; }

    public bool IsRewarded { get; set; }

    public bool PointsAwarded { get; set; }

    public int PointsAmount { get; set; }

    public Guid? UserDeviceId { get; set; }

    public UserDevice? UserDevice { get; set; }

    public Guid CommandId { get; set; }

    public Guid? DeviceId { get; set; }

    public DataSource Source { get; set; } = DataSource.Mobile;

    public DateTime? SyncedAt { get; set; }
}
