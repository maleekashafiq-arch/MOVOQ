using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class AdminUserRepository(MovoqDbContext db) : IAdminUserRepository
{
    public void Add(AdminUser admin) => db.AdminUsers.Add(admin);

    public Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.AdminUsers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        db.AdminUsers.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

    public async Task<IReadOnlyList<AdminUser>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.AdminUsers
            .AsNoTracking()
            .OrderBy(x => x.Email)
            .ToListAsync(cancellationToken);
}
