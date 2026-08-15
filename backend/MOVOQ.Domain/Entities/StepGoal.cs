namespace MOVOQ.Domain.Entities;

/// <summary>Per-user daily step target history. Global min/max/default live in <see cref="AppSetting"/>.</summary>
public sealed class StepGoal : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public int DailyTargetSteps { get; set; }

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; }
}
