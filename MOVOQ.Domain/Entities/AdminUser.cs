using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Back-office operator account, isolated from end <see cref="User"/> records.</summary>
public sealed class AdminUser : BaseEntity
{
    public required string Email { get; set; }

    public string? DisplayName { get; set; }

    public AdminRole Role { get; set; } = AdminRole.ReadOnly;

    public AdminStatus Status { get; set; } = AdminStatus.PendingActivation;

    public string? PasswordHashReference { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public ICollection<AuditLog> AuditLogs { get; } = new List<AuditLog>();
}
