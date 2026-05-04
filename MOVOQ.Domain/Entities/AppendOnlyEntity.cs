namespace MOVOQ.Domain.Entities;

/// <summary>
/// Base for rows that are inserted once and must not follow the normal editable/soft-deleted lifecycle.
/// </summary>
public abstract class AppendOnlyEntity
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// UTC instant when the row was created.
    /// When setting this in code, always use <see cref="DateTime.UtcNow"/> — never <see cref="DateTime.Now"/>.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optimistic concurrency token when the store supports it on insert-only tables.</summary>
    public int Version { get; set; }
}
