using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IPointsLedgerRepository
{
    void Add(PointsLedger entry);

    /// <summary>
    /// Sums every row a user has ever written. Cost grows with account age, so this is for reconciliation and
    /// audit only — balance reads belong on <c>UserWallet</c>, which is updated in the same transaction as
    /// every ledger write.
    /// </summary>
    Task<long> SumLifetimeAmountsAsync(Guid userId, CancellationToken cancellationToken = default);
}
