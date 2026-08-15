using System.Security.Cryptography;
using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

/// <summary>
/// Runs lucky draws from the back office. Winner selection is the sensitive part: it happens once, picks from
/// the tickets that were actually bought, and records how the choice was made in the audit trail.
/// </summary>
public sealed class AdminDrawsService(
    ILuckyDrawRepository draws,
    ILuckyDrawTicketRepository tickets,
    ILuckyDrawWinnerRepository winners,
    IUserRepository users,
    IAuditTrail audit,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IAdminDrawsService
{
    private const int MaxTicketsReturned = 200;

    public async Task<IReadOnlyList<AdminDrawDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var all = await draws.ListAsync(cancellationToken);
        var ids = all.Select(d => d.Id).ToList();

        var totals = (await tickets.GetTotalsAsync(ids, cancellationToken)).ToDictionary(t => t.LuckyDrawId);
        var won = (await winners.ListForDrawsAsync(ids, cancellationToken)).ToDictionary(w => w.LuckyDrawId);

        return all
            .Select(draw => Map(draw, totals.GetValueOrDefault(draw.Id), won.GetValueOrDefault(draw.Id)?.UserName))
            .ToList();
    }

    public async Task<AdminDrawDetailDto> GetAsync(Guid drawId, CancellationToken cancellationToken = default)
    {
        var draw = await draws.GetAsync(drawId, cancellationToken) ?? throw NotFound(drawId);
        return await DetailAsync(draw, cancellationToken);
    }

    public async Task<AdminDrawDto> CreateAsync(
        AdminContext admin,
        SaveDrawCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var draw = new LuckyDraw
        {
            Name = command.Name.Trim(),
            Description = Clean(command.Description),
            PrizeDescription = command.PrizeDescription.Trim(),
            Emoji = Clean(command.Emoji),
            ImageUrl = Clean(command.ImageUrl),
            TicketCostPoints = command.TicketCostPoints,
            StartAt = command.StartAt,
            EndAt = command.EndAt,
            MaxTickets = command.MaxTickets,
            IsFeatured = command.IsFeatured,
            Status = LuckyDrawStatus.Draft,
            CreatedAt = now,
        };

        draws.Add(draw);
        audit.Record(admin, AuditActionType.DrawModified, nameof(LuckyDraw), draw.Id, null, $"Created {draw.Name}");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(draw, null, null);
    }

    public async Task<AdminDrawDto> UpdateAsync(
        AdminContext admin,
        Guid drawId,
        SaveDrawCommand command,
        CancellationToken cancellationToken = default)
    {
        Validate(command);

        var draw = await draws.GetAsync(drawId, cancellationToken) ?? throw NotFound(drawId);
        if (draw.Status is LuckyDrawStatus.WinnerSelected or LuckyDrawStatus.Cancelled)
        {
            throw new AdminException(
                ErrorCodes.InvalidStateTransition,
                $"A {draw.Status.ToString().ToLowerInvariant()} draw can no longer be edited.",
                409);
        }

        var totals = await TotalsAsync(drawId, cancellationToken);
        if (command.MaxTickets is int cap && cap < totals.Tickets)
        {
            throw new AdminException(
                ErrorCodes.ValidationFailed,
                $"{totals.Tickets} tickets have already been sold, so the cap cannot be set below that.");
        }

        var before = $"{draw.Name}, {draw.TicketCostPoints} pts, ends {draw.EndAt:yyyy-MM-dd HH:mm}";

        draw.Name = command.Name.Trim();
        draw.Description = Clean(command.Description);
        draw.PrizeDescription = command.PrizeDescription.Trim();
        draw.Emoji = Clean(command.Emoji);
        draw.ImageUrl = Clean(command.ImageUrl);
        draw.TicketCostPoints = command.TicketCostPoints;
        draw.StartAt = command.StartAt;
        draw.EndAt = command.EndAt;
        draw.MaxTickets = command.MaxTickets;
        draw.IsFeatured = command.IsFeatured;
        draw.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;

        audit.Record(
            admin,
            AuditActionType.DrawModified,
            nameof(LuckyDraw),
            draw.Id,
            before,
            $"{draw.Name}, {draw.TicketCostPoints} pts, ends {draw.EndAt:yyyy-MM-dd HH:mm}");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(draw, totals, null);
    }

    public async Task<AdminDrawDto> ChangeStatusAsync(
        AdminContext admin,
        Guid drawId,
        LuckyDrawStatus status,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(status))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Unknown draw status.");
        }

        var draw = await draws.GetAsync(drawId, cancellationToken) ?? throw NotFound(drawId);
        if (draw.Status == status)
        {
            return await MapWithTotalsAsync(draw, cancellationToken);
        }

        if (!CanTransition(draw.Status, status))
        {
            throw new AdminException(
                ErrorCodes.InvalidStateTransition,
                $"A {draw.Status.ToString().ToLowerInvariant()} draw cannot become {status.ToString().ToLowerInvariant()}.",
                409);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var before = draw.Status;
        draw.Status = status;
        draw.UpdatedAt = now;

        // Cancelling voids the entries. Points are not returned: buyers are told at purchase time that a
        // ticket is spent, and refunding would need a policy decision this screen cannot make.
        if (status == LuckyDrawStatus.Cancelled)
        {
            foreach (var ticket in await tickets.ListActiveForDrawAsync(drawId, cancellationToken))
            {
                ticket.Status = TicketStatus.Cancelled;
                ticket.UpdatedAt = now;
            }
        }

        audit.Record(admin, AuditActionType.DrawModified, nameof(LuckyDraw), draw.Id, before.ToString(), status.ToString());

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await MapWithTotalsAsync(draw, cancellationToken);
    }

    public async Task<AdminDrawDetailDto> SelectWinnerAsync(
        AdminContext admin,
        Guid drawId,
        CancellationToken cancellationToken = default)
    {
        var draw = await draws.GetAsync(drawId, cancellationToken) ?? throw NotFound(drawId);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (draw.IsWinnerSelected || draw.Status == LuckyDrawStatus.WinnerSelected)
        {
            throw new AdminException(ErrorCodes.InvalidStateTransition, "This draw already has a winner.", 409);
        }

        if (draw.Status == LuckyDrawStatus.Cancelled)
        {
            throw new AdminException(ErrorCodes.InvalidStateTransition, "A cancelled draw cannot be drawn.", 409);
        }

        // An active draw whose end time has passed is finished in every sense but the flag, so close it here
        // rather than making the operator press two buttons.
        if (draw.Status == LuckyDrawStatus.Active && draw.EndAt <= now)
        {
            draw.Status = LuckyDrawStatus.Closed;
        }

        if (draw.Status != LuckyDrawStatus.Closed)
        {
            throw new AdminException(
                ErrorCodes.InvalidStateTransition,
                "Close the draw before picking a winner.",
                409);
        }

        var entries = await tickets.ListActiveForDrawAsync(drawId, cancellationToken);
        if (entries.Count == 0)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Nobody entered this draw.", 409);
        }

        var winning = entries[RandomNumberGenerator.GetInt32(entries.Count)];
        var user = await users.GetByIdForIdentityAsync(winning.UserId, cancellationToken);

        winners.Add(new LuckyDrawWinner
        {
            LuckyDrawId = draw.Id,
            LuckyDraw = draw,
            LuckyDrawTicketId = winning.Id,
            LuckyDrawTicket = winning,
            // The invariant the database cannot express: the winner is whoever holds the winning ticket.
            UserId = winning.UserId,
            SelectedAt = now,
            SelectionMethod = LuckyDrawOptions.RandomSelectionMethod,
            Notes = $"Drawn from {entries.Count} entries by {admin.Email}.",
            CreatedAt = now,
        });

        winning.Status = TicketStatus.Used;
        winning.UpdatedAt = now;
        draw.Status = LuckyDrawStatus.WinnerSelected;
        draw.IsWinnerSelected = true;
        draw.UpdatedAt = now;

        audit.Record(
            admin,
            AuditActionType.DrawWinnerSelected,
            nameof(LuckyDraw),
            draw.Id,
            $"{entries.Count} entries",
            $"Ticket {winning.TicketNumber} held by {user?.Email ?? winning.UserId.ToString()}");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await DetailAsync(draw, cancellationToken);
    }

    public async Task<AdminTicketsReportDto> GetTicketsAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var today = timeProvider.GetUtcNow().UtcDateTime.Date;
        var totals = await tickets.GetTotalsSinceAsync(today, cancellationToken);
        var recent = await tickets.ListRecentAsync(Math.Clamp(take, 1, MaxTicketsReturned), cancellationToken);

        return new AdminTicketsReportDto(
            totals.Tickets,
            totals.Buyers,
            totals.PointsSpent,
            await tickets.CountByStatusAsync(TicketStatus.Active, cancellationToken),
            await tickets.CountAsync(cancellationToken),
            recent
                .Select(t => new AdminTicketRowDto(
                    t.Id,
                    t.TicketNumber,
                    t.UserId,
                    t.UserName,
                    t.LuckyDrawId,
                    t.DrawName,
                    t.PointsCost,
                    t.Status,
                    t.PurchasedAt))
                .ToList());
    }

    private async Task<AdminDrawDetailDto> DetailAsync(LuckyDraw draw, CancellationToken cancellationToken)
    {
        var totals = await TotalsAsync(draw.Id, cancellationToken);
        var entries = await tickets.ListForDrawAsync(draw.Id, LuckyDrawOptions.MaxEntriesReturned, cancellationToken);
        var winner = (await winners.ListForDrawsAsync([draw.Id], cancellationToken)).FirstOrDefault();

        return new AdminDrawDetailDto(
            Map(draw, totals, winner?.UserName),
            entries
                .Select(e => new AdminDrawEntryDto(
                    e.Id,
                    e.TicketNumber,
                    e.UserId,
                    e.UserName,
                    e.PointsCost,
                    e.Status,
                    e.PurchasedAt))
                .ToList(),
            winner is null
                ? null
                : MapWinner(winner));
    }

    public async Task<AdminPrizesReportDto> GetPrizesAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var items = await winners.ListRecentAsync(Math.Clamp(take, 1, MaxTicketsReturned), cancellationToken);
        var awaiting = await winners.CountByFulfillmentAsync(PrizeFulfillmentStatus.AwaitingDetails, cancellationToken);
        var packed = await winners.CountByFulfillmentAsync(PrizeFulfillmentStatus.Packed, cancellationToken);
        var shipped = await winners.CountByFulfillmentAsync(PrizeFulfillmentStatus.Shipped, cancellationToken);
        var delivered = await winners.CountByFulfillmentAsync(PrizeFulfillmentStatus.Delivered, cancellationToken);
        return new AdminPrizesReportDto(
            awaiting,
            packed + shipped,
            delivered,
            await winners.CountAsync(cancellationToken),
            items.Select(MapWinner).ToList());
    }

    public async Task<AdminDrawWinnerDto> UpdateFulfillmentAsync(
        AdminContext admin,
        Guid winnerId,
        PrizeFulfillmentStatus status,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(status))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Unknown fulfillment status.");
        }

        var winner = await winners.GetAsync(winnerId, cancellationToken)
            ?? throw new AdminException(ErrorCodes.NotFound, $"No prize with id {winnerId}.", 404);

        var before = winner.FulfillmentStatus;
        winner.FulfillmentStatus = status;
        if (notes is not null)
        {
            winner.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        }

        audit.Record(
            admin,
            AuditActionType.PrizeFulfillmentUpdated,
            nameof(LuckyDrawWinner),
            winner.Id,
            before.ToString(),
            status.ToString());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var row = (await winners.ListForDrawsAsync([winner.LuckyDrawId], cancellationToken)).FirstOrDefault()
            ?? throw new AdminException(ErrorCodes.NotFound, $"No prize with id {winnerId}.", 404);
        return MapWinner(row);
    }

    private async Task<AdminDrawDto> MapWithTotalsAsync(LuckyDraw draw, CancellationToken cancellationToken)
    {
        var winner = (await winners.ListForDrawsAsync([draw.Id], cancellationToken)).FirstOrDefault();
        return Map(draw, await TotalsAsync(draw.Id, cancellationToken), winner?.UserName);
    }

    private async Task<DrawTicketTotals> TotalsAsync(Guid drawId, CancellationToken cancellationToken)
    {
        var totals = await tickets.GetTotalsAsync([drawId], cancellationToken);
        return totals.FirstOrDefault() ?? new DrawTicketTotals(drawId, 0, 0, 0);
    }

    private static bool CanTransition(LuckyDrawStatus from, LuckyDrawStatus to) => (from, to) switch
    {
        (LuckyDrawStatus.Draft, LuckyDrawStatus.Active) => true,
        (LuckyDrawStatus.Draft, LuckyDrawStatus.Cancelled) => true,
        (LuckyDrawStatus.Active, LuckyDrawStatus.Closed) => true,
        (LuckyDrawStatus.Active, LuckyDrawStatus.Cancelled) => true,
        (LuckyDrawStatus.Closed, LuckyDrawStatus.Cancelled) => true,
        _ => false,
    };

    private static void Validate(SaveDrawCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "A draw needs a name.");
        }

        if (string.IsNullOrWhiteSpace(command.PrizeDescription))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Describe the prize users are playing for.");
        }

        if (command.TicketCostPoints < 0)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "A ticket cannot cost less than zero points.");
        }

        if (command.EndAt <= command.StartAt)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "The draw must end after it starts.");
        }

        if (command.MaxTickets is int cap && cap < 1)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "A ticket cap must be at least one, or left empty.");
        }
    }

    private static AdminDrawDto Map(LuckyDraw draw, DrawTicketTotals? totals, string? winnerName) => new(
        draw.Id,
        draw.Name,
        draw.Description,
        draw.PrizeDescription,
        draw.Emoji,
        draw.ImageUrl,
        draw.TicketCostPoints,
        draw.StartAt,
        draw.EndAt,
        draw.Status,
        draw.MaxTickets,
        draw.IsFeatured,
        totals?.Tickets ?? 0,
        totals?.Participants ?? 0,
        totals?.PointsCollected ?? 0,
        draw.IsWinnerSelected,
        winnerName,
        draw.CreatedAt);

    private static AdminDrawWinnerDto MapWinner(WinnerRow winner) => new(
        winner.Id,
        winner.LuckyDrawId,
        winner.DrawName,
        winner.PrizeDescription,
        winner.TicketId,
        winner.TicketNumber,
        winner.UserId,
        winner.UserName,
        winner.SelectedAt,
        winner.SelectionMethod,
        winner.FulfillmentStatus,
        winner.Notes);

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AdminException NotFound(Guid drawId) =>
        new(ErrorCodes.NotFound, $"No draw with id {drawId}.", 404);
}
