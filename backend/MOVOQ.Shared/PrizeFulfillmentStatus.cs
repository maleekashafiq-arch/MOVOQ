namespace MOVOQ.Shared;

/// <summary>How far a lucky-draw prize has travelled from the draw result to the winner's hands.</summary>
public enum PrizeFulfillmentStatus
{
    AwaitingDetails = 1,
    Packed = 2,
    Shipped = 3,
    Delivered = 4
}
