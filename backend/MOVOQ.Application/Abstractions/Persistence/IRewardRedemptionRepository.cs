using MOVOQ.Shared;

namespace MOVOQ.Application.Abstractions.Persistence;

public sealed record RedemptionCount(Guid Id, int Count);

public interface IRewardRedemptionRepository
{
    Task<IReadOnlyList<RedemptionCount>> CountByRewardAsync(
        IReadOnlyCollection<Guid> rewardIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RedemptionCount>> CountBySponsorAsync(
        IReadOnlyCollection<Guid> sponsorIds,
        CancellationToken cancellationToken = default);

    Task<int> CountSinceAsync(DateTime since, CancellationToken cancellationToken = default);

    Task<long> PointsRedeemedSinceAsync(DateTime since, CancellationToken cancellationToken = default);
}
