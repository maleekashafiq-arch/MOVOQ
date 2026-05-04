using MOVOQ.Domain.Abstractions;
using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Registered mobile device for a user. Mobile-originated; sync-enabled.</summary>
public sealed class UserDevice : BaseEntity, ISyncableEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public required string DeviceIdentifier { get; set; }

    public DevicePlatform Platform { get; set; } = DevicePlatform.Unknown;

    public string? PushToken { get; set; }

    public string? DeviceModel { get; set; }

    public string? OsVersion { get; set; }

    public DateTime RegisteredAt { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public bool IsActive { get; set; } = true;

    public Guid CommandId { get; set; }

    public Guid? DeviceId { get; set; }

    public DataSource Source { get; set; } = DataSource.Mobile;

    public DateTime? SyncedAt { get; set; }
}
