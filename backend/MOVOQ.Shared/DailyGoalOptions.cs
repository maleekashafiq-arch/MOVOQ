namespace MOVOQ.Shared;

/// <summary>Allowed daily step targets collected at profile completion.</summary>
public static class DailyGoalOptions
{
    public const int Casual = 4000;

    public const int Active = 6000;

    public const int Pro = 10000;

    public const int Default = Active;

    public static readonly int[] Allowed = [Casual, Active, Pro];

    public static bool IsAllowed(int dailyTargetSteps) => Allowed.Contains(dailyTargetSteps);
}
