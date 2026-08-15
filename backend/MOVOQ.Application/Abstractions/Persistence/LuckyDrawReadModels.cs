using MOVOQ.Shared;

namespace MOVOQ.Application.Abstractions.Persistence;

/// <summary>Ticket aggregates for one draw. Participants counts people, tickets counts entries.</summary>
public sealed record DrawTicketTotals(Guid LuckyDrawId, int Tickets, int Participants, long PointsCollected);

public sealed record TicketRow(
    Guid Id,
    string TicketNumber,
    Guid UserId,
    string UserName,
    Guid LuckyDrawId,
    string DrawName,
    long PointsCost,
    TicketStatus Status,
    DateTime PurchasedAt);

public sealed record TicketDayTotals(int Tickets, int Buyers, long PointsSpent);

public sealed record WinnerRow(
    Guid Id,
    Guid LuckyDrawId,
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
