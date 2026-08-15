namespace MOVOQ.Shared;

/// <summary>
/// Ranking for <see cref="AdminRole"/>. The enum values run the other way (SuperAdmin = 1), so comparing
/// them directly would grant the least privileged operator the most access.
/// </summary>
public static class AdminRoles
{
    public static int RankOf(AdminRole role) => role switch
    {
        AdminRole.SuperAdmin => 4,
        AdminRole.Admin => 3,
        AdminRole.Support => 2,
        _ => 1,
    };

    /// <summary>True when <paramref name="role"/> is at least as privileged as <paramref name="required"/>.</summary>
    public static bool Satisfies(AdminRole role, AdminRole required) => RankOf(role) >= RankOf(required);
}
