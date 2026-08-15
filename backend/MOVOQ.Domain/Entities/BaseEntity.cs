namespace MOVOQ.Domain.Entities;

/// <summary>
/// Base persistence fields for domain entities. Timestamps use UTC.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// UTC instant when the entity was created.
    /// When setting this in code, always use <see cref="DateTime.UtcNow"/> — never <see cref="DateTime.Now"/>.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC instant of the last update, if any.
    /// When setting this in code, always use <see cref="DateTime.UtcNow"/> — never <see cref="DateTime.Now"/>.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>UTC instant of soft delete, if deleted; null when active.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Optimistic concurrency token; increment on each successful write.</summary>
    public int Version { get; set; }
}
