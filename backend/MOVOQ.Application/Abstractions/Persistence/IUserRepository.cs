using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IUserRepository
{
    void Add(User user);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> GetByIdForIdentityAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> ListForDevAsync(CancellationToken cancellationToken = default);
}
