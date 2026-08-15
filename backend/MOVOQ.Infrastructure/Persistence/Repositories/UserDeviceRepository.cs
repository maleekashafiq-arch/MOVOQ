using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class UserDeviceRepository(MovoqDbContext db) : IUserDeviceRepository
{
    public void Add(UserDevice device) => db.UserDevices.Add(device);

    public Task<UserDevice?> GetByUserAndIdentifierAsync(
        Guid userId,
        string deviceIdentifier,
        CancellationToken cancellationToken = default) =>
        db.UserDevices.SingleOrDefaultAsync(
            x => x.UserId == userId && x.DeviceIdentifier == deviceIdentifier,
            cancellationToken);
}
