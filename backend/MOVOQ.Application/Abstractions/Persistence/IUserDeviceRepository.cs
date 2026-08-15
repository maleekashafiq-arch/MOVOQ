using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IUserDeviceRepository
{
    void Add(UserDevice device);

    Task<UserDevice?> GetByUserAndIdentifierAsync(
        Guid userId,
        string deviceIdentifier,
        CancellationToken cancellationToken = default);
}
