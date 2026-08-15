using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>
/// Append-only ledger of point movements. This aggregate is the financial source of truth: wallet projections
/// and external balances must be derived or reconciled from successful rows here, never from client claims alone.
/// </summary>
public sealed class PointsLedger : AppendOnlyEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>Positive credits, negative debits.</summary>
    public long Amount { get; set; }

    public PointsTransactionType TransactionType { get; set; }

    public LedgerReferenceType ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public string? Description { get; set; }

    /// <summary>Business time of the movement (may differ from <see cref="AppendOnlyEntity.CreatedAt"/>).</summary>
    public DateTime OccurredAt { get; set; }
}
