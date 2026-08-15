namespace MOVOQ.Shared;

/// <summary>Auditable privileged actions.</summary>
public enum AuditActionType
{
    UserStatusChanged = 1,
    PointsAdjusted = 2,
    RewardModified = 3,
    AdminLogin = 4,
    DrawWinnerSelected = 5,
    FraudOverride = 6,
    SettingsUpdated = 7,
    AdminUserModified = 8,
    DrawModified = 9,
    SponsorModified = 10,
    PrizeFulfillmentUpdated = 11
}
