using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(MovoqDbContext db) : IUserRepository
{
    public void Add(User user) => db.Users.Add(user);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}
