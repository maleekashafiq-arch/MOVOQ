namespace MOVOQ.Domain.Entities;

/// <summary>Brand or sponsor offering marketplace rewards.</summary>
public sealed class Sponsor : BaseEntity
{
    public required string Name { get; set; }

    public string? ContactPersonName { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? WebsiteUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Reward> Rewards { get; } = new List<Reward>();
}
