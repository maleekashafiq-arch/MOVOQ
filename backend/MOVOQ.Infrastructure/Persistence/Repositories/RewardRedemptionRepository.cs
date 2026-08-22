using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class RewardRedemptionRepository(MovoqDbContext db) : IRewardRedemptionRepository
{
    public void Add(RewardRedemption redemption) => db.RewardRedemptions.Add(redemption);

    public async Task<IReadOnlyList<RewardRedemption>> ListForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await db.RewardRedemptions
            .AsNoTracking()
            .Include(x => x.Reward)
            .ThenInclude(r => r.Sponsor)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);

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
