using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Append-only ledger of point movements. Wallet projections must follow successful writes here.</summary>
public sealed class PointsLedger : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>Positive credits, negative debits.</summary>
    public int Amount { get; set; }

    public PointsTransactionType TransactionType { get; set; }

    public required string ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public string? Description { get; set; }

    /// <summary>Business time of the movement (may differ from <see cref="BaseEntity.CreatedAt"/>).</summary>
    public DateTime OccurredAt { get; set; }
}
