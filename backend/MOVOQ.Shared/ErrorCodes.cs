namespace MOVOQ.Shared;

/// <summary>Stable machine-readable error codes for API clients.</summary>
public static class ErrorCodes
{
    public const string ValidationFailed = "validation_failed";

    public const string AccountLinkConflict = "account_link_conflict";

    public const string SessionRequired = "session_required";

    public const string EmailRequired = "email_required";

    /// <summary>The Firebase account's email has not been verified, so it cannot be used for the back office.</summary>
    public const string EmailNotVerified = "email_not_verified";

    /// <summary>The day being synced is older than the accepted late-sync window, so retrying will not help.</summary>
    public const string SyncWindowClosed = "sync_window_closed";

    /// <summary>Authenticated, but the signed-in email is not a back-office operator.</summary>
    public const string AdminAccessDenied = "admin_access_denied";

    /// <summary>The operator is a back-office user but lacks the role this action requires.</summary>
    public const string AdminRoleRequired = "admin_role_required";

    /// <summary>The requested record does not exist or is no longer visible.</summary>
    public const string NotFound = "not_found";

    /// <summary>The feature is switched off in platform settings, so the whole area is unavailable.</summary>
    public const string FeatureDisabled = "feature_disabled";

    /// <summary>The wallet does not hold enough points to cover the purchase.</summary>
    public const string InsufficientPoints = "insufficient_points";

    /// <summary>The draw is not accepting entries: it is a draft, closed, cancelled, or outside its window.</summary>
    public const string DrawNotOpen = "draw_not_open";

    /// <summary>The draw has reached its ticket cap.</summary>
    public const string DrawSoldOut = "draw_sold_out";

    /// <summary>The user has bought as many tickets today as the daily limit allows.</summary>
    public const string TicketLimitReached = "ticket_limit_reached";

    /// <summary>The requested state change is not legal from the record's current state.</summary>
    public const string InvalidStateTransition = "invalid_state_transition";

    /// <summary>The reward is not on sale: draft, inactive, expired, or the sponsor is paused.</summary>
    public const string RewardUnavailable = "reward_unavailable";

    /// <summary>The reward has no remaining stock.</summary>
    public const string OutOfStock = "out_of_stock";

    /// <summary>Two writers changed the same row; the client should retry.</summary>
    public const string ConcurrencyConflict = "concurrency_conflict";

    /// <summary>The client is sending requests faster than the endpoint allows.</summary>
    public const string RateLimited = "rate_limited";
}
