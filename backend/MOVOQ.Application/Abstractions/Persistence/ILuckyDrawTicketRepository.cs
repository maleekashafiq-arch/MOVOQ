using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface ILuckyDrawTicketRepository
{
    void Add(LuckyDrawTicket ticket);

    /// <summary>Entry counts and takings per draw, for the draws asked about.</summary>
    Task<IReadOnlyList<DrawTicketTotals>> GetTotalsAsync(
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken = default);

    /// <summary>How many tickets one user holds in each of the given draws.</summary>
    Task<IReadOnlyDictionary<Guid, int>> CountForUserAsync(
        Guid userId,
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken = default);

    /// <summary>Tickets a user has bought since a moment, used to enforce the daily limit.</summary>
    Task<int> CountForUserSinceAsync(
        Guid userId,
        DateTime since,
        CancellationToken cancellationToken = default);

    /// <summary>Tracked active tickets, for picking a winner or voiding a cancelled draw.</summary>
    Task<IReadOnlyList<LuckyDrawTicket>> ListActiveForDrawAsync(
        Guid drawId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketRow>> ListForDrawAsync(
        Guid drawId,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketRow>> ListRecentAsync(int take, CancellationToken cancellationToken = default);

    Task<TicketDayTotals> GetTotalsSinceAsync(DateTime since, CancellationToken cancellationToken = default);

    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<int> CountByStatusAsync(TicketStatus status, CancellationToken cancellationToken = default);
}
