using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Features.Identity;

/// <summary>
/// Resolves the MOVOQ account behind an external token for features that need the entity rather than a DTO.
/// The returned user is tracked and includes the profile, wallet, and step goals.
/// </summary>
public interface IUserProvisioning
{
    Task<User> ResolveUserAsync(ExternalIdentity identity, CancellationToken cancellationToken = default);
}
