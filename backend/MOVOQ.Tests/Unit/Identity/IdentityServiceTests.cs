using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Application.Features.Identity;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Tests.Unit.Identity;

public sealed class IdentityServiceTests
{
    [Fact]
    public async Task ExchangeSession_creates_user_wallet_and_profile()
    {
        var harness = new Harness();
        var identity = new ExternalIdentity("uid-1", "Ahmed@Gmail.com", EmailVerified: false, "Ahmed");

        var dto = await harness.Sut.ExchangeSessionAsync(identity);

        Assert.Equal("ahmed@gmail.com", dto.Email);
        Assert.False(dto.EmailVerified);
        Assert.False(dto.ProfileComplete);
        Assert.Equal(0, dto.WalletBalance);
        Assert.Equal("Ahmed", dto.Profile?.DisplayName);
        Assert.Single(harness.Users.Items);
        Assert.Single(harness.AuthProviders.Items);
        Assert.NotNull(harness.Users.Items[0].Wallet);
    }

    [Fact]
    public async Task ExchangeSession_same_firebase_uid_does_not_duplicate()
    {
        var harness = new Harness();
        var identity = new ExternalIdentity("uid-1", "a@movoq.test", true, null);

        await harness.Sut.ExchangeSessionAsync(identity);
        await harness.Sut.ExchangeSessionAsync(identity);

        Assert.Single(harness.Users.Items);
        Assert.Single(harness.AuthProviders.Items);
        Assert.True(harness.Users.Items[0].EmailVerifiedAt.HasValue);
    }

    [Fact]
    public async Task ExchangeSession_links_when_existing_email_is_verified()
    {
        var harness = new Harness();
        var existing = new User { Email = "same@movoq.test", EmailVerifiedAt = DateTime.UtcNow };
        harness.Users.Add(existing);

        var dto = await harness.Sut.ExchangeSessionAsync(new ExternalIdentity("google-uid", "same@movoq.test", true, null));

        Assert.Equal(existing.Id, dto.Id);
        Assert.Single(harness.AuthProviders.Items);
    }

    [Fact]
    public async Task ExchangeSession_rejects_unverified_email_collision()
    {
        var harness = new Harness();
        harness.Users.Add(new User { Email = "same@movoq.test" });

        var ex = await Assert.ThrowsAsync<IdentityException>(() =>
            harness.Sut.ExchangeSessionAsync(new ExternalIdentity("other-uid", "same@movoq.test", false, null)));

        Assert.Equal(ErrorCodes.AccountLinkConflict, ex.Code);
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task CompleteProfile_requires_iso_country_and_allowed_goal()
    {
        var harness = new Harness();
        var identity = new ExternalIdentity("uid-1", "p@movoq.test", true, null);
        await harness.Sut.ExchangeSessionAsync(identity);

        await Assert.ThrowsAsync<IdentityException>(() =>
            harness.Sut.CompleteProfileAsync(identity, new CompleteProfileCommand("Ada", "Pakistan", "Punjab", "Lahore", null, null, 6000)));

        var dto = await harness.Sut.CompleteProfileAsync(
            identity,
            new CompleteProfileCommand("Ada Lovelace", "pk", "Punjab", "Lahore", "+923001112233", "Female", 6000));

        Assert.True(dto.ProfileComplete);
        Assert.Equal("PK", dto.Profile?.Country);
        Assert.Equal("Punjab", dto.Profile?.State);
        Assert.Equal(6000, dto.DailyTargetSteps);
        Assert.Equal("+923001112233", dto.PhoneNumber);
        Assert.Single(harness.StepGoals.Items);
    }

    [Fact]
    public async Task RegisterDevice_is_idempotent_for_same_identifier()
    {
        var harness = new Harness();
        var identity = new ExternalIdentity("uid-1", "d@movoq.test", true, null);
        await harness.Sut.ExchangeSessionAsync(identity);

        var first = await harness.Sut.RegisterDeviceAsync(
            identity,
            new RegisterDeviceCommand("pixel-8", DevicePlatform.Android, "token-a", "Pixel 8", "14", Guid.NewGuid()));
        var second = await harness.Sut.RegisterDeviceAsync(
            identity,
            new RegisterDeviceCommand("pixel-8", DevicePlatform.Android, "token-b", "Pixel 8", "15", Guid.NewGuid()));

        Assert.Equal(first.Id, second.Id);
        Assert.Single(harness.Devices.Items);
        Assert.Equal("token-b", harness.Devices.Items[0].PushToken);
    }

    private sealed class Harness
    {
        public FakeUserRepository Users { get; } = new();
        public FakeAuthProviderRepository AuthProviders { get; } = new();
        public FakeDeviceRepository Devices { get; } = new();
        public FakeStepGoalRepository StepGoals { get; } = new();
        public IdentityService Sut { get; }

        public Harness()
        {
            AuthProviders.Users = Users;
            Sut = new IdentityService(Users, AuthProviders, Devices, StepGoals, new FakeUnitOfWork());
        }
    }

    private sealed class FakeStepGoalRepository : IStepGoalRepository
    {
        public List<StepGoal> Items { get; } = [];

        public void Add(StepGoal goal) => Items.Add(goal);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

        public Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default) =>
            action(cancellationToken);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Items { get; } = [];

        public void Add(User user) => Items.Add(user);

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(x => x.Email == email));

        public Task<User?> GetByIdForIdentityAsync(Guid id, CancellationToken cancellationToken = default) =>
            GetByIdAsync(id, cancellationToken);

        public Task<IReadOnlyList<User>> ListForDevAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<User>>(Items);
    }

    private sealed class FakeAuthProviderRepository : IUserAuthProviderRepository
    {
        public List<UserAuthProvider> Items { get; } = [];
        public FakeUserRepository Users { get; set; } = null!;

        public void Add(UserAuthProvider authProvider)
        {
            authProvider.UserId = authProvider.User.Id;
            Items.Add(authProvider);
        }

        public Task<UserAuthProvider?> GetByProviderKeyAsync(
            AuthProvider provider,
            string providerUserId,
            CancellationToken cancellationToken = default)
        {
            var match = Items.SingleOrDefault(x => x.Provider == provider && x.ProviderUserId == providerUserId);
            if (match is not null)
            {
                match.User = Users.Items.Single(u => u.Id == match.UserId);
            }

            return Task.FromResult(match);
        }
    }

    private sealed class FakeDeviceRepository : IUserDeviceRepository
    {
        public List<UserDevice> Items { get; } = [];

        public void Add(UserDevice device) => Items.Add(device);

        public Task<UserDevice?> GetByUserAndIdentifierAsync(
            Guid userId,
            string deviceIdentifier,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(x => x.UserId == userId && x.DeviceIdentifier == deviceIdentifier));
    }
}
