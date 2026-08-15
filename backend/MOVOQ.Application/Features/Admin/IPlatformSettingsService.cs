using MOVOQ.Application.Features.Identity;

namespace MOVOQ.Application.Features.Admin;

/// <summary>
/// Reads and writes the operator-editable platform configuration stored in <c>app_settings</c>.
/// Any key that has never been saved falls back to the compiled default, so the platform behaves
/// identically before and after an operator first opens the settings screen.
/// </summary>
public interface IPlatformSettingsService
{
    Task<PlatformSettingsDto> GetAsync(CancellationToken cancellationToken = default);

    Task<PlatformSettingsDto> UpdateAsync(
        AdminContext admin,
        PlatformSettingsDto settings,
        CancellationToken cancellationToken = default);

    /// <summary>Reads a single numeric setting for runtime use, falling back when unset or unparsable.</summary>
    Task<int> GetIntAsync(string key, int fallback, CancellationToken cancellationToken = default);
}
