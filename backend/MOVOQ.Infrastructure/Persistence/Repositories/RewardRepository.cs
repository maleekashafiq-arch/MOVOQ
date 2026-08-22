using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class RewardRepository(MovoqDbContext db) : IRewardRepository
{
    public void Add(Reward reward) => db.Rewards.Add(reward);

    public Task<Reward?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Rewards
            .Include(x => x.Sponsor)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Reward?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await db.Database.ExecuteSqlAsync(
            $"""SELECT 1 FROM rewards WHERE "Id" = {id} AND "DeletedAt" IS NULL FOR UPDATE""",
            cancellationToken);
        return await GetAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<Reward>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.Rewards
            .AsNoTracking()
            .Include(x => x.Sponsor)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
}
