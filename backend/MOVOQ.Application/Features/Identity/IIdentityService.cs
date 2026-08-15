namespace MOVOQ.Application.Features.Identity;

public interface IIdentityService
{
    Task<CurrentUserDto> ExchangeSessionAsync(ExternalIdentity identity, CancellationToken cancellationToken = default);

    Task<CurrentUserDto> GetCurrentUserAsync(ExternalIdentity identity, CancellationToken cancellationToken = default);

    Task<CurrentUserDto> CompleteProfileAsync(
        ExternalIdentity identity,
        CompleteProfileCommand command,
        CancellationToken cancellationToken = default);

    Task<RegisteredDeviceDto> RegisterDeviceAsync(
        ExternalIdentity identity,
        RegisterDeviceCommand command,
        CancellationToken cancellationToken = default);
}
