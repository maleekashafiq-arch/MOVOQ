namespace MOVOQ.Shared;

/// <summary>
/// Keys for rows in <c>app_settings</c>. These are the knobs the back-office exposes, so the names are
/// part of the admin API contract and must not change once operators have saved values against them.
/// </summary>
public static class PlatformSettingKeys
{
    public const string AppName = "app.name";
    public const string SupportEmail = "app.supportEmail";

    public const string DailyGoalReward = "points.dailyGoalReward";
    public const string AdReward = "points.adReward";
    public const string StreakReward = "points.streakReward";
    public const string SpinMaxReward = "points.spinMax";

    public const string MaxPointsPerDay = "limits.maxPointsPerDay";
    public const string MaxTicketsPerDay = "limits.maxTicketsPerDay";
    public const string MaxAdsPerDay = "limits.maxAdsPerDay";

    public const string TicketConversionRate = "tickets.conversionRate";
    public const string TicketExpiryDays = "tickets.expiryDays";

    public const string AdFrequency = "ads.frequency";
    public const string AdsOnHome = "ads.home";
    public const string AdsOnMarketplace = "ads.marketplace";

    public const string CoachPrice = "aiCoach.price";
    public const string CoachTrialDays = "aiCoach.trialDays";

    public const string FeatureNotifications = "features.notifications";
    public const string FeatureAds = "features.ads";
    public const string FeatureLuckyDraws = "features.luckyDraws";
    public const string FeatureTeams = "features.teams";
    public const string FeatureAiCoach = "features.aiCoach";
}
