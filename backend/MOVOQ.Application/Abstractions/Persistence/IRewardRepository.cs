using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IRewardRepository
{
    void Add(Reward reward);

    Task<Reward?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reward>> ListAsync(CancellationToken cancellationToken = default);
}
