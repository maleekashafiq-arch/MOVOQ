namespace MOVOQ.Shared;

/// <summary>Classification of a points ledger movement.</summary>
public enum PointsTransactionType
{
    StepsEarned = 1,
    AdEarned = 2,
    RedemptionDeducted = 3,
    TicketDeducted = 4,
    AdminAdjustment = 5,
    Expired = 6,
    Reversed = 7
}
