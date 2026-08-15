using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

/// <summary>
/// Creates the first operator accounts from configuration. Without this there is no way in: the admin API
/// only trusts emails already present in <c>admin_users</c>, and nothing else writes that table.
/// </summary>
public interface IAdminBootstrapper
{
    Task<int> EnsureOperatorsAsync(
        IEnumerable<string> emails,
        AdminRole role,
        CancellationToken cancellationToken = default);
}
