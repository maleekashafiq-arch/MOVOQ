using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class AuditLogRepository(MovoqDbContext db) : IAuditLogRepository
{
    public void Add(AuditLog entry) => db.AuditLogs.Add(entry);

    public async Task<IReadOnlyList<AuditLog>> ListRecentAsync(int take, CancellationToken cancellationToken = default) =>
        await db.AuditLogs
            .AsNoTracking()
            .Include(x => x.AdminUser)
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
}
