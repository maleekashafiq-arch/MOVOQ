using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Ticket issued by the server after points validation.</summary>
public sealed class LuckyDrawTicket : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid LuckyDrawId { get; set; }

    public LuckyDraw LuckyDraw { get; set; } = null!;

    public required string TicketNumber { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.Active;

    public int PointsCostSnapshot { get; set; }

    public Guid? PointsLedgerId { get; set; }

    public PointsLedger? PointsLedger { get; set; }

    public DateTime PurchasedAt { get; set; }

    public LuckyDrawWinner? Winner { get; set; }
}
