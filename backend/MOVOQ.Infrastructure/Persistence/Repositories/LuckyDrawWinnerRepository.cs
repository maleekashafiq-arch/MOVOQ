using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class LuckyDrawWinnerRepository(MovoqDbContext db) : ILuckyDrawWinnerRepository
{
    public void Add(LuckyDrawWinner winner) => db.LuckyDrawWinners.Add(winner);

    public Task<LuckyDrawWinner?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.LuckyDrawWinners.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<IReadOnlyList<WinnerRow>> ListForDrawsAsync(
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken = default) =>
        drawIds.Count == 0
            ? Task.FromResult<IReadOnlyList<WinnerRow>>([])
            : ReadAsync(db.LuckyDrawWinners.AsNoTracking().Where(x => drawIds.Contains(x.LuckyDrawId)), null, cancellationToken);

    public Task<IReadOnlyList<WinnerRow>> ListRecentAsync(int take, CancellationToken cancellationToken = default) =>
        ReadAsync(db.LuckyDrawWinners.AsNoTracking(), take, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        db.LuckyDrawWinners.CountAsync(cancellationToken);

    public Task<int> CountByFulfillmentAsync(
        PrizeFulfillmentStatus status,
        CancellationToken cancellationToken = default) =>
        db.LuckyDrawWinners.CountAsync(x => x.FulfillmentStatus == status, cancellationToken);

    private static async Task<IReadOnlyList<WinnerRow>> ReadAsync(
        IQueryable<LuckyDrawWinner> source,
        int? take,
        CancellationToken cancellationToken)
    {
        var ordered = source.OrderByDescending(x => x.SelectedAt);
        var limited = take is null ? ordered : ordered.Take(take.Value);

        var rows = await limited
            .Select(x => new
            {
                x.Id,
                x.LuckyDrawId,
                DrawName = x.LuckyDraw.Name,
                PrizeDescription = x.LuckyDraw.PrizeDescription,
                TicketId = x.LuckyDrawTicketId,
                x.LuckyDrawTicket.TicketNumber,
                x.UserId,
                DisplayName = x.User.Profile != null ? x.User.Profile.DisplayName : null,
                x.User.Email,
                x.SelectedAt,
                x.SelectionMethod,
                x.FulfillmentStatus,
                x.Notes,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new WinnerRow(
                r.Id,
                r.LuckyDrawId,
                r.DrawName,
                r.PrizeDescription,
                r.TicketId,
                r.TicketNumber,
                r.UserId,
                DisplayName(r.DisplayName, r.Email),
                r.SelectedAt,
                r.SelectionMethod,
                r.FulfillmentStatus,
                r.Notes))
            .ToList();
    }

    private static string DisplayName(string? displayName, string email)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        var at = email.IndexOf('@');
        return at > 0 ? email[..at] : email;
    }
}
