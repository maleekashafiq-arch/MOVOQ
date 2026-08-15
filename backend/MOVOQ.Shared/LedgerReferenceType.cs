namespace MOVOQ.Shared;

/// <summary>Typed discriminator for <c>PointsLedger.ReferenceId</c> to avoid free-text reference drift.</summary>
public enum LedgerReferenceType
{
    DailyStepRecord = 1,
    AdsWatched = 2,
    RewardRedemption = 3,
    LuckyDrawTicket = 4,
    AdminAdjustment = 5,
    StreakBonus = 6,
    ManualCorrection = 7
}
