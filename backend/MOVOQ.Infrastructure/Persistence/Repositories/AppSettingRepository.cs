using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class AppSettingRepository(MovoqDbContext db) : IAppSettingRepository
{
    public void Add(AppSetting setting) => db.AppSettings.Add(setting);

    // Tracked on purpose: the settings service edits the rows it reads back.
    public async Task<IReadOnlyList<AppSetting>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.AppSettings.ToListAsync(cancellationToken);
}
