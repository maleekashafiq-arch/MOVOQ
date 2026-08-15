using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IUserAuthProviderRepository
{
    void Add(UserAuthProvider authProvider);

    Task<UserAuthProvider?> GetByProviderKeyAsync(
        AuthProvider provider,
        string providerUserId,
        CancellationToken cancellationToken = default);
}
