namespace MOVOQ.Shared;

/// <summary>Processing state of a points transaction.</summary>
public enum PointsTransactionStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3,
    Reversed = 4,
    Expired = 5
}
