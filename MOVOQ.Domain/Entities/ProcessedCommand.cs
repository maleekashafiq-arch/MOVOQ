namespace MOVOQ.Domain.Entities;

/// <summary>
/// Global idempotency receipt for offline/mobile commands. One logical <see cref="CommandId"/> must not be applied twice across entity types.
/// </summary>
/// <remarks>
/// Uniqueness of <see cref="CommandId"/> must be enforced at the database layer (unique index).
/// </remarks>
public sealed class ProcessedCommand : AppendOnlyEntity
{
    /// <summary>Client-generated idempotency key (UUID v4 per ADR).</summary>
    public Guid CommandId { get; set; }

    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public Guid? DeviceId { get; set; }

    public required string CommandType { get; set; }

    public required string Status { get; set; }

    public DateTime ProcessedAt { get; set; }

    public string? RelatedEntityName { get; set; }

    public Guid? RelatedEntityId { get; set; }
}
