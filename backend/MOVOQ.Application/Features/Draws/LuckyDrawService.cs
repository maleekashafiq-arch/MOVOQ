using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Application.Features.Admin;
using MOVOQ.Application.Features.Identity;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Draws;

/// <summary>
/// Sells lucky draw tickets. Every ticket is paid for with a ledger debit written in the same transaction as
/// the wallet update, so a ticket can always be traced back to the points that bought it.
/// </summary>
public sealed class LuckyDrawService(
    IUserProvisioning users,
    ILuckyDrawRepository draws,
    ILuckyDrawTicketRepository tickets,
    IUserWalletRepository wallets,
    IPointsLedgerRepository ledger,
    IProcessedCommandRepository processedCommands,
    IPlatformSettingsService settings,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : ILuckyDrawService
{
    private const string BuyCommandType = "draws.buy";

    public async Task<LuckyDrawListDto> ListAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        await RequireFeatureAsync(cancellationToken);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var user = await users.ResolveUserAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var open = await draws.ListOpenAsync(now, cancellationToken);
        var ids = open.Select(d => d.Id).ToList();

        var totals = (await tickets.GetTotalsAsync(ids, cancellationToken)).ToDictionary(t => t.LuckyDrawId);
        var mine = await tickets.CountForUserAsync(user.Id, ids, cancellationToken);

        return new LuckyDrawListDto(
            open.Select(draw => Map(draw, totals.GetValueOrDefault(draw.Id), mine.GetValueOrDefault(draw.Id))).ToList(),
            user.Wallet?.Balance ?? 0,
            await tickets.CountForUserSinceAsync(user.Id, now.Date, cancellationToken),
            await DailyLimitAsync(cancellationToken));
    }

    public async Task<BuyTicketResultDto> BuyTicketsAsync(
        ExternalIdentity identity,
        Guid drawId,
        BuyTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        await RequireFeatureAsync(cancellationToken);
        ValidateCommand(command);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var user = await users.ResolveUserAsync(identity, cancellationToken);

        // A replayed command must not sell a second set of tickets, so answer from stored state instead.
        if (await processedCommands.ExistsByCommandIdAsync(command.CommandId, cancellationToken))
        {
            return await ReadStateAsync(user, drawId, cancellationToken) with { Duplicate = true };
        }

        var draw = await draws.GetAsync(drawId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "That draw no longer exists.", 404);

        RequireOpen(draw, now);

        var totals = await TotalsAsync(drawId, cancellationToken);
        if (draw.MaxTickets is int cap && totals.Tickets + command.Quantity > cap)
        {
            var left = Math.Max(cap - totals.Tickets, 0);
            throw new AppException(
                ErrorCodes.DrawSoldOut,
                left == 0
                    ? "This draw has sold out."
                    : $"Only {left} ticket{(left == 1 ? string.Empty : "s")} left in this draw.",
                409);
        }

        var limit = await DailyLimitAsync(cancellationToken);
        var boughtToday = await tickets.CountForUserSinceAsync(user.Id, now.Date, cancellationToken);
        if (limit > 0 && boughtToday + command.Quantity > limit)
        {
            throw new AppException(
                ErrorCodes.TicketLimitReached,
                $"You can buy {limit} tickets a day, and you have bought {boughtToday} today.",
                409);
        }

        var wallet = EnsureWallet(user);
        var cost = draw.TicketCostPoints * command.Quantity;
        if (wallet.Balance < cost)
        {
            throw new AppException(
                ErrorCodes.InsufficientPoints,
                $"That costs {cost} points and your balance is {wallet.Balance}.",
                409);
        }

        var issued = new List<string>(command.Quantity);
        LuckyDrawTicket? last = null;
        for (var i = 0; i < command.Quantity; i++)
        {
            last = IssueTicket(user, draw, now);
            issued.Add(last.TicketNumber);
        }

        wallet.Balance -= cost;
        wallet.LifetimeSpentPoints += cost;
        wallet.UpdatedAt = now;

        processedCommands.Add(new ProcessedCommand
        {
            CommandId = command.CommandId,
            UserId = user.Id,
            CommandType = BuyCommandType,
            Status = "Applied",
            ProcessedAt = now,
            RelatedEntityName = nameof(LuckyDrawTicket),
            RelatedEntityId = last?.Id,
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var mine = await tickets.CountForUserAsync(user.Id, [drawId], cancellationToken);
        return new BuyTicketResultDto(
            drawId,
            issued,
            mine.GetValueOrDefault(drawId),
            totals.Tickets + command.Quantity,
            cost,
            wallet.Balance,
            Duplicate: false);
    }

    private LuckyDrawTicket IssueTicket(User user, LuckyDraw draw, DateTime now)
    {
        var ticket = new LuckyDrawTicket
        {
            UserId = user.Id,
            User = user,
            LuckyDrawId = draw.Id,
            LuckyDraw = draw,
            TicketNumber = TicketNumbers.Next(LuckyDrawOptions.TicketNumberLength),
            Status = TicketStatus.Active,
            PointsCostSnapshot = draw.TicketCostPoints,
            PurchasedAt = now,
            CreatedAt = now,
        };

        // A zero-cost draw is a valid operator choice; skip the ledger rather than writing an empty entry.
        if (draw.TicketCostPoints > 0)
        {
            var entry = new PointsLedger
            {
                UserId = user.Id,
                User = user,
                Amount = -draw.TicketCostPoints,
                TransactionType = PointsTransactionType.TicketDeducted,
                ReferenceType = LedgerReferenceType.LuckyDrawTicket,
                ReferenceId = ticket.Id,
                Description = $"Ticket {ticket.TicketNumber} for {draw.Name}.",
                OccurredAt = now,
            };
            ledger.Add(entry);
            ticket.PointsLedger = entry;
        }

        tickets.Add(ticket);
        return ticket;
    }

    private async Task<BuyTicketResultDto> ReadStateAsync(
        User user,
        Guid drawId,
        CancellationToken cancellationToken)
    {
        var totals = await TotalsAsync(drawId, cancellationToken);
        var mine = await tickets.CountForUserAsync(user.Id, [drawId], cancellationToken);

        return new BuyTicketResultDto(
            drawId,
            [],
            mine.GetValueOrDefault(drawId),
            totals.Tickets,
            PointsSpent: 0,
            user.Wallet?.Balance ?? 0,
            Duplicate: false);
    }

    private async Task<DrawTicketTotals> TotalsAsync(Guid drawId, CancellationToken cancellationToken)
    {
        var totals = await tickets.GetTotalsAsync([drawId], cancellationToken);
        return totals.FirstOrDefault() ?? new DrawTicketTotals(drawId, 0, 0, 0);
    }

    private UserWallet EnsureWallet(User user)
    {
        if (user.Wallet is not null)
        {
            return user.Wallet;
        }

        // Explicit insert: a wallet reached only through the loaded user would be tracked as an update.
        var wallet = new UserWallet { UserId = user.Id, User = user };
        wallets.Add(wallet);
        user.Wallet = wallet;
        return wallet;
    }

    private Task<int> DailyLimitAsync(CancellationToken cancellationToken) =>
        settings.GetIntAsync(
            PlatformSettingKeys.MaxTicketsPerDay,
            PlatformSettingsService.Defaults.MaxTicketsPerDay,
            cancellationToken);

    private async Task RequireFeatureAsync(CancellationToken cancellationToken)
    {
        var enabled = await settings.GetAsync(cancellationToken);
        if (!enabled.FeatureLuckyDraws)
        {
            throw new AppException(ErrorCodes.FeatureDisabled, "Lucky draws are currently switched off.", 403);
        }
    }

    private static void RequireOpen(LuckyDraw draw, DateTime now)
    {
        var message = draw.Status switch
        {
            LuckyDrawStatus.Draft => "This draw has not opened yet.",
            LuckyDrawStatus.Closed or LuckyDrawStatus.WinnerSelected => "Entries for this draw have closed.",
            LuckyDrawStatus.Cancelled => "This draw was cancelled.",
            _ when draw.StartAt > now => "This draw has not opened yet.",
            _ when draw.EndAt <= now => "Entries for this draw have closed.",
            _ => null,
        };

        if (message is not null)
        {
            throw new AppException(ErrorCodes.DrawNotOpen, message, 409);
        }
    }

    private static void ValidateCommand(BuyTicketCommand command)
    {
        if (command.CommandId == Guid.Empty)
        {
            throw new AppException(ErrorCodes.ValidationFailed, "CommandId is required.");
        }

        if (command.Quantity < 1 || command.Quantity > LuckyDrawOptions.MaxTicketsPerPurchase)
        {
            throw new AppException(
                ErrorCodes.ValidationFailed,
                $"Buy between 1 and {LuckyDrawOptions.MaxTicketsPerPurchase} tickets at a time.");
        }
    }

    private static LuckyDrawDto Map(LuckyDraw draw, DrawTicketTotals? totals, int mine) => new(
        draw.Id,
        draw.Name,
        draw.Description,
        draw.PrizeDescription,
        draw.Emoji,
        draw.ImageUrl,
        draw.TicketCostPoints,
        draw.StartAt,
        draw.EndAt,
        totals?.Participants ?? 0,
        totals?.Tickets ?? 0,
        draw.MaxTickets,
        mine,
        draw.IsFeatured);
}
