using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IAdminUserRepository
{
    void Add(AdminUser admin);

    Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AdminUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminUser>> ListAsync(CancellationToken cancellationToken = default);
}
