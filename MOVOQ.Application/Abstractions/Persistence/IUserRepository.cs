using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IUserRepository
{
    void Add(User user);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
