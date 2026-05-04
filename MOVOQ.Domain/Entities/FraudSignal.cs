using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Server-generated fraud or risk signal. Append-only in MVP.</summary>
public sealed class FraudSignal : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public FraudSignalType SignalType { get; set; }

    public FraudSignalSeverity Severity { get; set; }

    public required string Description { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public DateTime DetectedAt { get; set; }
}
