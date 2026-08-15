namespace MOVOQ.Domain.Entities;

/// <summary>Global configuration key/value (e.g. step goal bounds, offline window).</summary>
public sealed class AppSetting : BaseEntity
{
    public required string Key { get; set; }

    public required string Value { get; set; }

    public string? Description { get; set; }

    public Guid? LastModifiedBy { get; set; }
}
