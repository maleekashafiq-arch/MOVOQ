namespace MOVOQ.Shared;

/// <summary>
/// Canonical HTTP path prefixes for MOVOQ HTTP APIs.
/// Keep mobile and admin clients aligned with the server contract.
/// </summary>
public static class ApiRoutePrefixes
{
    /// <summary>Versioned business APIs (ADR: avoid unversioned business routes).</summary>
    public const string V1 = "/api/v1";

    /// <summary>Unversioned operational endpoint for load balancers and orchestrators.</summary>
    public const string Health = "/health";
}
