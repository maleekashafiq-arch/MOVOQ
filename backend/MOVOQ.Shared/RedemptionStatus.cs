namespace MOVOQ.Shared;

/// <summary>State of a reward redemption workflow.</summary>
public enum RedemptionStatus
{
    Pending = 1,
    Confirmed = 2,
    Failed = 3,
    Cancelled = 4
}
