using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class LuckyDrawRepository(MovoqDbContext db) : ILuckyDrawRepository
{
    public void Add(LuckyDraw draw) => db.LuckyDraws.Add(draw);

    public Task<LuckyDraw?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.LuckyDraws.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<LuckyDraw>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.LuckyDraws
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LuckyDraw>> ListOpenAsync(
        DateTime now,
        CancellationToken cancellationToken = default) =>
        await db.LuckyDraws
            .AsNoTracking()
            .Where(x => x.Status == LuckyDrawStatus.Active && x.StartAt <= now && x.EndAt > now)
            // Featured first, then whichever closes soonest, so the app's list needs no sorting of its own.
            .OrderByDescending(x => x.IsFeatured)
            .ThenBy(x => x.EndAt)
            .ToListAsync(cancellationToken);
}
