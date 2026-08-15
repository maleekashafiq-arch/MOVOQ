using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IAppSettingRepository
{
    void Add(AppSetting setting);

    Task<IReadOnlyList<AppSetting>> ListAsync(CancellationToken cancellationToken = default);
}
