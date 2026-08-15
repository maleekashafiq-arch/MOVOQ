using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(MovoqDbContext db) : IUserRepository
{
    public void Add(User user) => db.Users.Add(user);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        IdentityQuery().SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<User?> GetByIdForIdentityAsync(Guid id, CancellationToken cancellationToken = default) =>
        IdentityQuery().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<User>> ListForDevAsync(CancellationToken cancellationToken = default) =>
        await IdentityQuery()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    private IQueryable<User> IdentityQuery() =>
        db.Users
            .Include(x => x.Profile)
            .Include(x => x.Wallet)
            .Include(x => x.StepGoals)
            .Include(x => x.AuthProviders);
}
