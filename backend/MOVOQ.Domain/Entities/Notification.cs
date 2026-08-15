using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Server-generated inbox item for a user.</summary>
public sealed class Notification : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public NotificationType Type { get; set; }

    public required string Title { get; set; }

    public required string Message { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }
}
