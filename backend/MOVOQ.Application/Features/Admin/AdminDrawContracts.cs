using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

public sealed record AdminDrawDto(
    Guid Id,
    string Name,
    string? Description,
    string PrizeDescription,
    string? Emoji,
    string? ImageUrl,
    long TicketCostPoints,
    DateTime StartAt,
    DateTime EndAt,
    LuckyDrawStatus Status,
    int? MaxTickets,
    bool IsFeatured,
    int Tickets,
    int Participants,
    long PointsCollected,
    bool IsWinnerSelected,
    string? WinnerName,
    DateTime CreatedAt);

public sealed record AdminDrawEntryDto(
    Guid TicketId,
    string TicketNumber,
    Guid UserId,
    string UserName,
    long PointsCost,
    TicketStatus Status,
    DateTime PurchasedAt);

public sealed record AdminDrawWinnerDto(
    Guid Id,
    Guid DrawId,
    string DrawName,
    string PrizeDescription,
    Guid TicketId,
    string TicketNumber,
    Guid UserId,
    string UserName,
    DateTime SelectedAt,
    string? SelectionMethod,
    PrizeFulfillmentStatus FulfillmentStatus,
    string? Notes);

public sealed record AdminDrawDetailDto(
    AdminDrawDto Draw,
    IReadOnlyList<AdminDrawEntryDto> Entries,
    AdminDrawWinnerDto? Winner);

public sealed record SaveDrawCommand(
    string Name,
    string? Description,
    string PrizeDescription,
    string? Emoji,
    string? ImageUrl,
    long TicketCostPoints,
    DateTime StartAt,
    DateTime EndAt,
    int? MaxTickets,
    bool IsFeatured);

public sealed record AdminTicketRowDto(
    Guid Id,
    string TicketNumber,
    Guid UserId,
    string UserName,
    Guid DrawId,
    string DrawName,
    long PointsCost,
    TicketStatus Status,
    DateTime PurchasedAt);

public sealed record AdminTicketsReportDto(
    int TicketsToday,
    int BuyersToday,
    long PointsSpentToday,
    int ActiveTickets,
    int TicketsAllTime,
    IReadOnlyList<AdminTicketRowDto> Recent);

public sealed record AdminPrizesReportDto(
    int AwaitingDetails,
    int InTransit,
    int Delivered,
    int AwardedAllTime,
    IReadOnlyList<AdminDrawWinnerDto> Items);
