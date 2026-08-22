namespace MOVOQ.Shared;

/// <summary>Server-side rules that turn a synced step day into points.</summary>
public static class StepRewardOptions
{
    /// <summary>Credited once per day, the first time the server confirms the daily goal was reached.</summary>
    public const int GoalCompletionPoints = 50;

    /// <summary>A single day above this is treated as implausible and earns nothing.</summary>
    public const int MaxPlausibleDailySteps = 50_000;

    /// <summary>
    /// Peak sustainable cadence is well below this. A jump faster than this between two device
    /// timestamps is treated as spoofed and earns nothing.
    /// </summary>
    public const int MaxPlausibleStepsPerMinute = 250;

    /// <summary>10,000 steps in five minutes is not walking; reject even if the per-minute average is noisy.</summary>
    public const int MaxStepsInFiveMinutes = 10_000;

    /// <summary>Device clocks may sit a few minutes ahead of the server.</summary>
    public const int DeviceClockSkewMinutes = 5;

    /// <summary>How far back a device may still sync a closed day after being offline.</summary>
    public const int LateSyncWindowDays = 7;

    /// <summary>Days of history loaded to recompute the streak on every sync.</summary>
    public const int StreakWindowDays = 60;

    /// <summary>Upper bound for a history request.</summary>
    public const int MaxHistoryDays = 90;
}
