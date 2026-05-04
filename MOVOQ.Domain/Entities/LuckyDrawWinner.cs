namespace MOVOQ.Domain.Entities;

/// <summary>Immutable winner record for a completed draw.</summary>
/// <remarks>
/// Invariant: <see cref="UserId"/> must always match the <see cref="UserId"/> on the winning <see cref="LuckyDrawTicket"/>.
/// Enforce in Application and with a database check constraint when persistence is wired.
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

    public string? Notes { get; set; }
}
