namespace MOVOQ.Application.Features.Draws;

/// <summary>One draw as the app shows it, including how many tickets this user already holds.</summary>
public sealed record LuckyDrawDto(
    Guid Id,
    string Name,
    string? Description,
    string PrizeDescription,
    string? Emoji,
    string? ImageUrl,
    long TicketCostPoints,
    DateTime StartAt,
    DateTime EndAt,
    int Participants,
    int TotalTickets,
    int? MaxTickets,
    int MyTickets,
    bool Featured);

/// <summary>The draw list plus the wallet state the app needs to enable or disable the buy buttons.</summary>
public sealed record LuckyDrawListDto(
    IReadOnlyList<LuckyDrawDto> Draws,
    long WalletBalance,
    int TicketsBoughtToday,
    // Zero means the operator has not capped daily purchases.
    int MaxTicketsPerDay);

public sealed record BuyTicketCommand(Guid CommandId, int Quantity);

/// <summary>
/// Everything the app needs to repaint after a purchase. <c>Duplicate</c> marks a replayed command, which
/// returns the state the first attempt produced rather than buying a second time.
/// </summary>
public sealed record BuyTicketResultDto(
    Guid DrawId,
    IReadOnlyList<string> TicketNumbers,
    int MyTickets,
    int TotalTickets,
    long PointsSpent,
    long WalletBalance,
    bool Duplicate);
