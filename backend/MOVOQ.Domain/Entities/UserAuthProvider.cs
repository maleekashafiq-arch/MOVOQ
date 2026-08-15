using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Links an external identity (Firebase UID) to a <see cref="User"/>.</summary>
public sealed class UserAuthProvider : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public AuthProvider Provider { get; set; } = AuthProvider.Firebase;

    public required string ProviderUserId { get; set; }

    public string? EmailAtLink { get; set; }

    public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
}
