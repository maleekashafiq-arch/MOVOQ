using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IFraudSignalRepository
{
    void Add(FraudSignal signal);

    Task<IReadOnlyList<FraudSignal>> ListRecentAsync(int take, CancellationToken cancellationToken = default);

    Task<int> CountSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken = default);
}
