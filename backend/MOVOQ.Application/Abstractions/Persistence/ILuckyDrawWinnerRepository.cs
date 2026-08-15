using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface ILuckyDrawWinnerRepository
{
    void Add(LuckyDrawWinner winner);

    /// <summary>Tracked, for fulfillment updates.</summary>
    Task<LuckyDrawWinner?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Winners of the given draws. A draw has at most one, enforced by a unique index.</summary>
    Task<IReadOnlyList<WinnerRow>> ListForDrawsAsync(
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WinnerRow>> ListRecentAsync(int take, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<int> CountByFulfillmentAsync(
        PrizeFulfillmentStatus status,
        CancellationToken cancellationToken = default);
}
