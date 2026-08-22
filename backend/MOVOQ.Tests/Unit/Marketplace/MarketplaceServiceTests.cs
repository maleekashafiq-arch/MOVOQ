using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Application.Features.Identity;
using MOVOQ.Application.Features.Marketplace;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;
using MOVOQ.Tests.Unit.Admin.Fakes;

namespace MOVOQ.Tests.Unit.Marketplace;

public sealed class MarketplaceServiceTests
{
    private static readonly DateTime Now = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ExternalIdentity Identity = new("uid-1", "walker@movoq.test", true, "Walker");

    [Fact]
    public async Task List_returns_only_active_in_window_rewards()
    {
        var harness = new Harness();
        harness.AddReward("Gym pass", 400, RewardStatus.Active);
        harness.AddReward("Draft mug", 100, RewardStatus.Draft);

        var list = await harness.Sut.ListAsync(Identity);

        Assert.Single(list.Rewards);
        Assert.Equal("Gym pass", list.Rewards[0].Name);
        Assert.Equal(500, list.WalletBalance);
    }

    [Fact]
    public async Task Redeem_debits_wallet_and_issues_a_code()
    {
        var harness = new Harness();
        var reward = harness.AddReward("Gym pass", 400, RewardStatus.Active, stock: 2);

        var result = await harness.Sut.RedeemAsync(Identity, reward.Id, new RedeemCommand(Guid.NewGuid()));

        Assert.False(result.Duplicate);
        Assert.Equal(400, result.PointsSpent);
        Assert.Equal(100, result.WalletBalance);
        Assert.Equal(100, harness.User.Wallet!.Balance);
        Assert.NotNull(result.AssignedCode);
        Assert.StartsWith("MVQ-", result.AssignedCode);
        Assert.Single(harness.Redemptions.Items);
        Assert.Equal(1, reward.StockQuantity);
        Assert.Single(harness.Ledger.Items);
        Assert.Equal(PointsTransactionType.RedemptionDeducted, harness.Ledger.Items[0].TransactionType);
    }

    [Fact]
    public async Task Redeem_is_idempotent_on_command_id()
    {
        var harness = new Harness();
        var reward = harness.AddReward("Gym pass", 400, RewardStatus.Active);
        var command = new RedeemCommand(Guid.NewGuid());

        await harness.Sut.RedeemAsync(Identity, reward.Id, command);
        var replay = await harness.Sut.RedeemAsync(Identity, reward.Id, command);

        Assert.True(replay.Duplicate);
        Assert.Equal(0, replay.PointsSpent);
        Assert.Equal(100, harness.User.Wallet!.Balance);
        Assert.Single(harness.Redemptions.Items);
    }

    [Fact]
    public async Task Redeem_rejects_insufficient_points()
    {
        var harness = new Harness();
        harness.User.Wallet!.Balance = 50;
        var reward = harness.AddReward("Gym pass", 400, RewardStatus.Active);

        var error = await Assert.ThrowsAsync<MOVOQ.Application.Abstractions.AppException>(
            () => harness.Sut.RedeemAsync(Identity, reward.Id, new RedeemCommand(Guid.NewGuid())));

        Assert.Equal(ErrorCodes.InsufficientPoints, error.Code);
        Assert.Empty(harness.Redemptions.Items);
    }

    private sealed class Harness
    {
        public Harness()
        {
            User = new User { Email = Identity.Email! };
            User.Wallet = new UserWallet { UserId = User.Id, User = User, Balance = 500 };
            Sponsor = new Sponsor { Name = "FitLife", IsActive = true, CreatedAt = Now };

            Sut = new MarketplaceService(
                new FakeUserProvisioning(User),
                Rewards,
                Redemptions,
                Wallets,
                Ledger,
                ProcessedCommands,
                new FakeUnitOfWork(),
                new FixedTimeProvider(Now));
        }

        public User User { get; }

        public Sponsor Sponsor { get; }

        public FakeRewardRepository Rewards { get; } = new();

        public FakeRewardRedemptionRepository Redemptions { get; } = new();

        public FakeWalletRepository Wallets { get; } = new();

        public FakeLedgerRepository Ledger { get; } = new();

        public FakeProcessedCommandRepository ProcessedCommands { get; } = new();

        public MarketplaceService Sut { get; }

        public Reward AddReward(string name, long cost, RewardStatus status, int? stock = 5)
        {
            var reward = new Reward
            {
                SponsorId = Sponsor.Id,
                Sponsor = Sponsor,
                Name = name,
                PointsCost = cost,
                Status = status,
                StockQuantity = stock,
                CreatedAt = Now,
            };
            Rewards.Add(reward);
            return reward;
        }
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    private sealed class FakeUserProvisioning(User user) : IUserProvisioning
    {
        public Task<User> ResolveUserAsync(ExternalIdentity identity, CancellationToken cancellationToken = default) =>
            Task.FromResult(user);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

        public Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> action,
            CancellationToken cancellationToken = default) =>
            action(cancellationToken);
    }

    private sealed class FakeWalletRepository : IUserWalletRepository
    {
        public List<UserWallet> Items { get; } = [];

        public void Add(UserWallet wallet) => Items.Add(wallet);

        public Task LockByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeLedgerRepository : IPointsLedgerRepository
    {
        public List<PointsLedger> Items { get; } = [];

        public void Add(PointsLedger entry) => Items.Add(entry);

        public Task<long> SumLifetimeAmountsAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Where(x => x.UserId == userId).Sum(x => x.Amount));
    }

    private sealed class FakeProcessedCommandRepository : IProcessedCommandRepository
    {
        public List<ProcessedCommand> Items { get; } = [];

        public void Add(ProcessedCommand processedCommand) => Items.Add(processedCommand);

        public Task<bool> ExistsByCommandIdAsync(Guid commandId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Any(x => x.CommandId == commandId));
    }
}
