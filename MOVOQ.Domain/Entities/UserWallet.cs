namespace MOVOQ.Domain.Entities;

/// <summary>
/// Cached wallet aggregates maintained by the server after ledger writes. Ledger remains source of truth.
/// </summary>
public sealed class UserWallet : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public long Balance { get; set; }

    public long LifetimeEarnedPoints { get; set; }

    public long LifetimeSpentPoints { get; set; }
}
