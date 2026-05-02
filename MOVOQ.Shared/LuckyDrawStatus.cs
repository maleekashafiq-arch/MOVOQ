namespace MOVOQ.Shared;

/// <summary>Lifecycle of a lucky draw campaign.</summary>
public enum LuckyDrawStatus
{
    Draft = 1,
    Active = 2,
    Closed = 3,
    WinnerSelected = 4,
    Cancelled = 5
}
