using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Append-only admin/system audit entry. Use <see cref="BaseEntity.CreatedAt"/> as the authoritative timestamp.</summary>
public sealed class AuditLog : BaseEntity
{
    public Guid? AdminUserId { get; set; }

    public AdminUser? AdminUser { get; set; }

    public AuditActionType ActionType { get; set; }

    public required string EntityName { get; set; }

    public Guid? EntityId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? IpAddress { get; set; }
}
