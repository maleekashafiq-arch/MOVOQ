using MOVOQ.Application.Features.Identity;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

/// <summary>
/// Turns an authenticated Firebase identity into a back-office operator. Being signed in proves who you
/// are; membership of <c>admin_users</c> is what proves you may be here.
/// </summary>
public interface IAdminAccessService
{
    /// <summary>Resolves the operator and fails when they are missing, inactive, or under-privileged.</summary>
    Task<AdminContext> RequireAsync(
        ExternalIdentity identity,
        AdminRole minimumRole,
        CancellationToken cancellationToken = default);

    /// <summary>Resolves the operator, stamps the login, and audits it.</summary>
    Task<AdminSessionDto> SignInAsync(ExternalIdentity identity, CancellationToken cancellationToken = default);
}
