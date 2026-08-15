using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class LuckyDrawTicketRepository(MovoqDbContext db) : ILuckyDrawTicketRepository
{
    public void Add(LuckyDrawTicket ticket) => db.LuckyDrawTickets.Add(ticket);

    public async Task<IReadOnlyList<DrawTicketTotals>> GetTotalsAsync(
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken = default)
    {
        if (drawIds.Count == 0)
        {
            return [];
        }

        var rows = await db.LuckyDrawTickets
            .AsNoTracking()
            .Where(x => drawIds.Contains(x.LuckyDrawId) && x.Status != TicketStatus.Cancelled)
            .GroupBy(x => x.LuckyDrawId)
            .Select(g => new
            {
                LuckyDrawId = g.Key,
                Tickets = g.Count(),
                Participants = g.Select(x => x.UserId).Distinct().Count(),
                Points = g.Sum(x => x.PointsCostSnapshot),
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new DrawTicketTotals(r.LuckyDrawId, r.Tickets, r.Participants, r.Points))
            .ToList();
    }

    public async Task<IReadOnlyDictionary<Guid, int>> CountForUserAsync(
        Guid userId,
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken = default)
    {
        if (drawIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        var rows = await db.LuckyDrawTickets
            .AsNoTracking()
            .Where(x => x.UserId == userId
                && drawIds.Contains(x.LuckyDrawId)
                && x.Status != TicketStatus.Cancelled)
            .GroupBy(x => x.LuckyDrawId)
            .Select(g => new { LuckyDrawId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.LuckyDrawId, r => r.Count);
    }

    public Task<int> CountForUserSinceAsync(
        Guid userId,
        DateTime since,
        CancellationToken cancellationToken = default) =>
        db.LuckyDrawTickets
            .AsNoTracking()
            .CountAsync(
                x => x.UserId == userId && x.PurchasedAt >= since && x.Status != TicketStatus.Cancelled,
                cancellationToken);

    public async Task<IReadOnlyList<LuckyDrawTicket>> ListActiveForDrawAsync(
        Guid drawId,
        CancellationToken cancellationToken = default) =>
        await db.LuckyDrawTickets
            .Where(x => x.LuckyDrawId == drawId && x.Status == TicketStatus.Active)
            .OrderBy(x => x.PurchasedAt)
            .ToListAsync(cancellationToken);

    public Task<IReadOnlyList<TicketRow>> ListForDrawAsync(
        Guid drawId,
        int take,
        CancellationToken cancellationToken = default) =>
        ReadAsync(db.LuckyDrawTickets.AsNoTracking().Where(x => x.LuckyDrawId == drawId), take, cancellationToken);

    public Task<IReadOnlyList<TicketRow>> ListRecentAsync(int take, CancellationToken cancellationToken = default) =>
        ReadAsync(db.LuckyDrawTickets.AsNoTracking(), take, cancellationToken);

    public async Task<TicketDayTotals> GetTotalsSinceAsync(
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        var totals = await db.LuckyDrawTickets
            .AsNoTracking()
            .Where(x => x.PurchasedAt >= since && x.Status != TicketStatus.Cancelled)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Tickets = g.Count(),
                Buyers = g.Select(x => x.UserId).Distinct().Count(),
                Points = g.Sum(x => x.PointsCostSnapshot),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return totals is null
            ? new TicketDayTotals(0, 0, 0)
            : new TicketDayTotals(totals.Tickets, totals.Buyers, totals.Points);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        db.LuckyDrawTickets.CountAsync(cancellationToken);

    public Task<int> CountByStatusAsync(TicketStatus status, CancellationToken cancellationToken = default) =>
        db.LuckyDrawTickets.CountAsync(x => x.Status == status, cancellationToken);

    private static async Task<IReadOnlyList<TicketRow>> ReadAsync(
        IQueryable<LuckyDrawTicket> source,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await source
            .OrderByDescending(x => x.PurchasedAt)
            .Take(take)
            .Select(x => new
            {
                x.Id,
                x.TicketNumber,
                x.UserId,
                DisplayName = x.User.Profile != null ? x.User.Profile.DisplayName : null,
                x.User.Email,
                x.LuckyDrawId,
                DrawName = x.LuckyDraw.Name,
                x.PointsCostSnapshot,
                x.Status,
                x.PurchasedAt,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new TicketRow(
                r.Id,
                r.TicketNumber,
                r.UserId,
                DisplayName(r.DisplayName, r.Email),
                r.LuckyDrawId,
                r.DrawName,
                r.PointsCostSnapshot,
                r.Status,
                r.PurchasedAt))
            .ToList();
    }

    /// <summary>Accounts that never completed a profile still need a label in admin tables.</summary>
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
