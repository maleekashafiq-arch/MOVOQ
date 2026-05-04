using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class PointsLedgerRepository(MovoqDbContext db) : IPointsLedgerRepository
{
    public void Add(PointsLedger entry) => db.PointsLedgers.Add(entry);

    public async Task<long> GetNetBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await db.PointsLedgers
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .SumAsync(e => (long)e.Amount, cancellationToken);
    }
}
