using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IPointsLedgerRepository
{
    void Add(PointsLedger entry);

    Task<long> GetNetBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
}
