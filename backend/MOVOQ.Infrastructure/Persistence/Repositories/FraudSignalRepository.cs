using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class FraudSignalRepository(MovoqDbContext db) : IFraudSignalRepository
{
    public void Add(FraudSignal signal) => db.FraudSignals.Add(signal);

    public async Task<IReadOnlyList<FraudSignal>> ListRecentAsync(int take, CancellationToken cancellationToken = default) =>
        await db.FraudSignals
            .AsNoTracking()
            .OrderByDescending(x => x.DetectedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken = default) =>
        db.FraudSignals.CountAsync(x => x.DetectedAt >= sinceUtc, cancellationToken);
}
