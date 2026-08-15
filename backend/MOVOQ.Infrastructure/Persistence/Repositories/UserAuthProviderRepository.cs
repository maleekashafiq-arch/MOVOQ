using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class UserAuthProviderRepository(MovoqDbContext db) : IUserAuthProviderRepository
{
    public void Add(UserAuthProvider authProvider) => db.UserAuthProviders.Add(authProvider);

    public Task<UserAuthProvider?> GetByProviderKeyAsync(
        AuthProvider provider,
        string providerUserId,
        CancellationToken cancellationToken = default) =>
        db.UserAuthProviders
            .Include(x => x.User)
                .ThenInclude(u => u.Profile)
            .Include(x => x.User)
                .ThenInclude(u => u.Wallet)
            .Include(x => x.User)
                .ThenInclude(u => u.StepGoals)
            .SingleOrDefaultAsync(
                x => x.Provider == provider && x.ProviderUserId == providerUserId,
                cancellationToken);
}
