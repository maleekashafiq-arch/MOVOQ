using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Identity;

public sealed record CurrentUserDto(
    Guid Id,
    string Email,
    bool EmailVerified,
    bool PhoneVerified,
    bool ProfileComplete,
    string? PhoneNumber,
    UserStatus Status,
    ProfileDto? Profile,
    long WalletBalance,
    int? DailyTargetSteps);

public sealed record ProfileDto(
    string? DisplayName,
    string? Country,
    string? State,
    string? City,
    string? Gender);

public sealed record CompleteProfileCommand(
    string FullName,
    string Country,
    string State,
    string City,
    string? PhoneNumber,
    string? Gender,
    int DailyTargetSteps);

public sealed record RegisterDeviceCommand(
    string DeviceIdentifier,
    DevicePlatform Platform,
    string? PushToken,
    string? DeviceModel,
    string? OsVersion,
    Guid CommandId);

public sealed record RegisteredDeviceDto(
    Guid Id,
    string DeviceIdentifier,
    DevicePlatform Platform,
    bool IsActive);
