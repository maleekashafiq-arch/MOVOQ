using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface ILuckyDrawRepository
{
    void Add(LuckyDraw draw);

    /// <summary>Tracked, for writes. Tickets are not loaded; ask the ticket repository for those.</summary>
    Task<LuckyDraw?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Every draw the back office can see, newest first.</summary>
    Task<IReadOnlyList<LuckyDraw>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Draws the app may sell tickets for right now: active and inside their window.</summary>
    Task<IReadOnlyList<LuckyDraw>> ListOpenAsync(DateTime now, CancellationToken cancellationToken = default);
}
