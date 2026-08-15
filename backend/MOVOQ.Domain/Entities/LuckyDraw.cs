using MOVOQ.Shared;

namespace MOVOQ.Domain.Entities;

/// <summary>Lucky draw campaign where users spend points for tickets.</summary>
public sealed class LuckyDraw : BaseEntity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public required string PrizeDescription { get; set; }

    /// <summary>Shown as the prize artwork in the app until <see cref="ImageUrl"/> is set.</summary>
    public string? Emoji { get; set; }

    public string? ImageUrl { get; set; }

    public long TicketCostPoints { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public LuckyDrawStatus Status { get; set; } = LuckyDrawStatus.Draft;

    public int? MaxTickets { get; set; }

    /// <summary>Given the hero treatment at the top of the app's draw list.</summary>
    public bool IsFeatured { get; set; }

    public bool IsWinnerSelected { get; set; }

    public ICollection<LuckyDrawTicket> Tickets { get; } = new List<LuckyDrawTicket>();

    public LuckyDrawWinner? Winner { get; set; }
}
