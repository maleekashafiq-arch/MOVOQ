using MOVOQ.Application.Abstractions.Persistence;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class EfUnitOfWork(MovoqDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
