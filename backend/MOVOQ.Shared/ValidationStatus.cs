namespace MOVOQ.Shared;

/// <summary>Server-side validation outcome for sync-originated financial precursors (steps, ads).</summary>
public enum ValidationStatus
{
    Pending = 1,
    Validated = 2,
    Rejected = 3
}
