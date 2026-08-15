using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Immutable winner record for a completed draw.</summary>
/// <remarks>
/// Invariant: <see cref="UserId"/> must always match the <see cref="UserId"/> on the winning <see cref="LuckyDrawTicket"/>.
/// Enforce in Application; optional DB CHECK on providers that allow subqueries in CHECK (not SQLite — use triggers or SQL Server–specific migrations if required).
/// </remarks>
public sealed class LuckyDrawWinner : AppendOnlyEntity
{
    public Guid LuckyDrawId { get; set; }

    public LuckyDraw LuckyDraw { get; set; } = null!;

    public Guid LuckyDrawTicketId { get; set; }

    public LuckyDrawTicket LuckyDrawTicket { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public DateTime SelectedAt { get; set; }

    public string? SelectionMethod { get; set; }

    public PrizeFulfillmentStatus FulfillmentStatus { get; set; } = PrizeFulfillmentStatus.AwaitingDetails;

    public string? Notes { get; set; }
}
