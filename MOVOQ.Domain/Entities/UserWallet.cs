namespace MOVOQ.Domain.Entities;

/// <summary>
/// Cached wallet aggregates maintained by the server after ledger writes. Ledger remains source of truth.
/// </summary>
public sealed class UserWallet : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public int Balance { get; set; }

    public int LifetimeEarnedPoints { get; set; }

    public int LifetimeSpentPoints { get; set; }
}
