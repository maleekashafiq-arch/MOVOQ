using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IAuditLogRepository
{
    void Add(AuditLog entry);

    Task<IReadOnlyList<AuditLog>> ListRecentAsync(int take, CancellationToken cancellationToken = default);
}
