namespace MOVOQ.Shared;

/// <summary>Lifecycle state of a user account.</summary>
public enum UserStatus
{
    PendingVerification = 1,
    Active = 2,
    Suspended = 3,
    Deleted = 4
}
