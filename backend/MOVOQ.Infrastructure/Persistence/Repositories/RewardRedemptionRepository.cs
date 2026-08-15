using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Shared;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class RewardRedemptionRepository(MovoqDbContext db) : IRewardRedemptionRepository
{
    public async Task<IReadOnlyList<RedemptionCount>> CountByRewardAsync(
        IReadOnlyCollection<Guid> rewardIds,
        CancellationToken cancellationToken = default)
    {
        if (rewardIds.Count == 0)
        {
            return [];
        }

        var rows = await db.RewardRedemptions
            .AsNoTracking()
            .Where(x => rewardIds.Contains(x.RewardId) && x.Status != RedemptionStatus.Cancelled)
            .GroupBy(x => x.RewardId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new RedemptionCount(r.Key, r.Count)).ToList();
    }

    public async Task<IReadOnlyList<RedemptionCount>> CountBySponsorAsync(
        IReadOnlyCollection<Guid> sponsorIds,
        CancellationToken cancellationToken = default)
    {
        if (sponsorIds.Count == 0)
        {
            return [];
        }

        var rows = await db.RewardRedemptions
            .AsNoTracking()
            .Where(x => sponsorIds.Contains(x.Reward.SponsorId) && x.Status != RedemptionStatus.Cancelled)
            .GroupBy(x => x.Reward.SponsorId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new RedemptionCount(r.Key, r.Count)).ToList();
    }

    public Task<int> CountSinceAsync(DateTime since, CancellationToken cancellationToken = default) =>
        db.RewardRedemptions
            .AsNoTracking()
            .CountAsync(
                x => x.RequestedAt >= since && x.Status != RedemptionStatus.Cancelled,
                cancellationToken);

    public async Task<long> PointsRedeemedSinceAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        var total = await db.RewardRedemptions
            .AsNoTracking()
            .Where(x => x.RequestedAt >= since && x.Status == RedemptionStatus.Confirmed)
            .SumAsync(x => (long?)x.PointsCostSnapshot, cancellationToken);

        return total ?? 0;
    }
}
