namespace MOVOQ.Domain.Entities;

/// <summary>Supplementary profile data for a <see cref="User"/>. Server-controlled.</summary>
public sealed class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Country { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public string? PreferredLanguage { get; set; }
}
