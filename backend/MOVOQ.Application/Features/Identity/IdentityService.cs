using System.Text.RegularExpressions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Identity;

public sealed class IdentityService(
    IUserRepository users,
    IUserAuthProviderRepository authProviders,
    IUserDeviceRepository devices,
    IStepGoalRepository stepGoals,
    IUnitOfWork unitOfWork) : IIdentityService, IUserProvisioning
{
    private static readonly Regex CountryCodePattern = new("^[A-Za-z]{2}$", RegexOptions.Compiled);

    public Task<User> ResolveUserAsync(ExternalIdentity identity, CancellationToken cancellationToken = default) =>
        LoadOrProvisionAsync(identity, touchLastLogin: false, cancellationToken);

    public Task<CurrentUserDto> ExchangeSessionAsync(ExternalIdentity identity, CancellationToken cancellationToken = default) =>
        EnsureUserAsync(identity, touchLastLogin: true, cancellationToken);

    public Task<CurrentUserDto> GetCurrentUserAsync(ExternalIdentity identity, CancellationToken cancellationToken = default) =>
        EnsureUserAsync(identity, touchLastLogin: false, cancellationToken);

    public async Task<CurrentUserDto> CompleteProfileAsync(
        ExternalIdentity identity,
        CompleteProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await LoadOrProvisionAsync(identity, touchLastLogin: false, cancellationToken);
        ApplyProfile(user, command);
        user.UpdatedAt = DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    public async Task<RegisteredDeviceDto> RegisterDeviceAsync(
        ExternalIdentity identity,
        RegisterDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.DeviceIdentifier))
        {
            throw new IdentityException(ErrorCodes.ValidationFailed, "Device identifier is required.");
        }

        if (command.CommandId == Guid.Empty)
        {
            throw new IdentityException(ErrorCodes.ValidationFailed, "CommandId is required.");
        }

        var user = await LoadOrProvisionAsync(identity, touchLastLogin: false, cancellationToken);
        var identifier = command.DeviceIdentifier.Trim();
        var existing = await devices.GetByUserAndIdentifierAsync(user.Id, identifier, cancellationToken);
        var now = DateTime.UtcNow;

        if (existing is null)
        {
            existing = new UserDevice
            {
                UserId = user.Id,
                User = user,
                DeviceIdentifier = identifier,
                Platform = command.Platform,
                PushToken = TrimToNull(command.PushToken),
                DeviceModel = TrimToNull(command.DeviceModel),
                OsVersion = TrimToNull(command.OsVersion),
                RegisteredAt = now,
                LastSeenAt = now,
                IsActive = true,
                CommandId = command.CommandId,
                DeviceId = null,
                Source = DataSource.Mobile,
                SyncedAt = now,
            };
            devices.Add(existing);
        }
        else
        {
            existing.Platform = command.Platform;
            existing.PushToken = TrimToNull(command.PushToken) ?? existing.PushToken;
            existing.DeviceModel = TrimToNull(command.DeviceModel) ?? existing.DeviceModel;
            existing.OsVersion = TrimToNull(command.OsVersion) ?? existing.OsVersion;
            existing.LastSeenAt = now;
            existing.IsActive = true;
            existing.CommandId = command.CommandId;
            existing.SyncedAt = now;
            existing.UpdatedAt = now;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new RegisteredDeviceDto(existing.Id, existing.DeviceIdentifier, existing.Platform, existing.IsActive);
    }

    private async Task<CurrentUserDto> EnsureUserAsync(
        ExternalIdentity identity,
        bool touchLastLogin,
        CancellationToken cancellationToken)
    {
        var user = await LoadOrProvisionAsync(identity, touchLastLogin, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    private async Task<User> LoadOrProvisionAsync(
        ExternalIdentity identity,
        bool touchLastLogin,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(identity.Email);
        if (email is null)
        {
            throw new IdentityException(ErrorCodes.EmailRequired, "A verified or unverified email is required to use MOVOQ.", 400);
        }

        if (string.IsNullOrWhiteSpace(identity.ProviderUserId))
        {
            throw new IdentityException(ErrorCodes.ValidationFailed, "External identity id is required.");
        }

        var link = await authProviders.GetByProviderKeyAsync(
            AuthProvider.Firebase,
            identity.ProviderUserId,
            cancellationToken);

        User user;
        if (link is not null)
        {
            user = link.User;
        }
        else
        {
            var existing = await users.GetByEmailAsync(email, cancellationToken);
            if (existing is not null)
            {
                if (existing.EmailVerifiedAt is null && !identity.EmailVerified)
                {
                    throw new IdentityException(
                        ErrorCodes.AccountLinkConflict,
                        "An account already exists for this email. Sign in with the original method, or verify the email before linking Google.",
                        409);
                }

                user = existing;
            }
            else
            {
                user = CreateUser(email, identity);
                users.Add(user);
            }

            authProviders.Add(new UserAuthProvider
            {
                User = user,
                UserId = user.Id,
                Provider = AuthProvider.Firebase,
                ProviderUserId = identity.ProviderUserId,
                EmailAtLink = email,
                LinkedAt = DateTime.UtcNow,
            });
        }

        SyncVerification(user, identity, email);
        if (touchLastLogin)
        {
            user.LastLoginAt = DateTime.UtcNow;
        }

        return user;
    }

    private static User CreateUser(string email, ExternalIdentity identity)
    {
        var user = new User
        {
            Email = email,
            EmailVerifiedAt = identity.EmailVerified ? DateTime.UtcNow : null,
            PasswordHashReference = $"firebase:{identity.ProviderUserId}",
            Status = UserStatus.Active,
            Role = UserRole.User,
        };

        user.Profile = new UserProfile
        {
            User = user,
            UserId = user.Id,
            DisplayName = TrimToNull(identity.DisplayName),
        };

        user.Wallet = new UserWallet
        {
            User = user,
            UserId = user.Id,
        };

        return user;
    }

    private static void SyncVerification(User user, ExternalIdentity identity, string email)
    {
        user.Email = email;
        if (identity.EmailVerified && user.EmailVerifiedAt is null)
        {
            user.EmailVerifiedAt = DateTime.UtcNow;
        }

        if (user.Profile is not null
            && string.IsNullOrWhiteSpace(user.Profile.DisplayName)
            && !string.IsNullOrWhiteSpace(identity.DisplayName))
        {
            user.Profile.DisplayName = identity.DisplayName.Trim();
        }
    }

    private void ApplyProfile(User user, CompleteProfileCommand command)
    {
        var fullName = command.FullName?.Trim() ?? string.Empty;
        if (fullName.Length is < 2 or > 256)
        {
            throw new IdentityException(ErrorCodes.ValidationFailed, "Full name must be between 2 and 256 characters.");
        }

        var country = command.Country?.Trim() ?? string.Empty;
        if (!CountryCodePattern.IsMatch(country))
        {
            throw new IdentityException(ErrorCodes.ValidationFailed, "Country must be a 2-letter ISO code (for example PK).");
        }

        var state = command.State?.Trim() ?? string.Empty;
        if (state.Length is < 1 or > 128)
        {
            throw new IdentityException(ErrorCodes.ValidationFailed, "State is required.");
        }

        var city = command.City?.Trim() ?? string.Empty;
        if (city.Length is < 1 or > 128)
        {
            throw new IdentityException(ErrorCodes.ValidationFailed, "City is required.");
        }

        if (!DailyGoalOptions.IsAllowed(command.DailyTargetSteps))
        {
            throw new IdentityException(
                ErrorCodes.ValidationFailed,
                "Daily goal must be 4000, 6000, or 10000 steps.");
        }

        var phone = TrimToNull(command.PhoneNumber);
        if (phone is { Length: > 32 })
        {
            throw new IdentityException(ErrorCodes.ValidationFailed, "Phone number is too long.");
        }

        var gender = TrimToNull(command.Gender);
        if (gender is { Length: > 32 })
        {
            throw new IdentityException(ErrorCodes.ValidationFailed, "Gender is too long.");
        }

        user.Profile ??= new UserProfile { User = user, UserId = user.Id };
        user.Profile.DisplayName = fullName;
        user.Profile.Country = country.ToUpperInvariant();
        user.Profile.State = state;
        user.Profile.City = city;
        user.Profile.Gender = gender;
        user.Profile.UpdatedAt = DateTime.UtcNow;

        user.PhoneNumber = phone;

        var now = DateTime.UtcNow;
        var activeGoal = user.StepGoals.SingleOrDefault(g => g.IsActive);
        if (activeGoal is null || activeGoal.DailyTargetSteps != command.DailyTargetSteps)
        {
            if (activeGoal is not null)
            {
                activeGoal.IsActive = false;
                activeGoal.EffectiveTo = now;
                activeGoal.UpdatedAt = now;
            }

            var goal = new StepGoal
            {
                User = user,
                UserId = user.Id,
                DailyTargetSteps = command.DailyTargetSteps,
                EffectiveFrom = now,
                IsActive = true,
            };

            // Entities carry a client-generated Id, so a goal discovered only through the loaded user's
            // collection is tracked as an update to a row that does not exist. Insert it explicitly and
            // only join it to the collection when the tracker has not already done so.
            stepGoals.Add(goal);
            if (!user.StepGoals.Contains(goal))
            {
                user.StepGoals.Add(goal);
            }
        }
    }

    private static CurrentUserDto Map(User user)
    {
        var profile = user.Profile;
        var complete = profile is not null
            && !string.IsNullOrWhiteSpace(profile.DisplayName)
            && !string.IsNullOrWhiteSpace(profile.Country)
            && !string.IsNullOrWhiteSpace(profile.State)
            && !string.IsNullOrWhiteSpace(profile.City);

        return new CurrentUserDto(
            user.Id,
            user.Email,
            user.EmailVerifiedAt is not null,
            user.PhoneVerifiedAt is not null,
            complete,
            user.PhoneNumber,
            user.Status,
            profile is null
                ? null
                : new ProfileDto(profile.DisplayName, profile.Country, profile.State, profile.City, profile.Gender),
            user.Wallet?.Balance ?? 0,
            user.StepGoals.SingleOrDefault(g => g.IsActive)?.DailyTargetSteps);
    }

    private static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return email.Trim().ToLowerInvariant();
    }

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
