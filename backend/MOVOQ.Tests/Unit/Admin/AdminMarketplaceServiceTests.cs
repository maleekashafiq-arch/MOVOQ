using MOVOQ.Application.Features.Admin;
using MOVOQ.Shared;
using MOVOQ.Tests.Unit.Admin.Fakes;

namespace MOVOQ.Tests.Unit.Admin;

public sealed class AdminMarketplaceServiceTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly AdminContext Operator = new(Guid.NewGuid(), "ops@movoq.test", "Ops", AdminRole.Admin);

    [Fact]
    public async Task Creating_a_sponsor_puts_them_on_the_directory()
    {
        var harness = new Harness();

        var created = await harness.Sut.CreateSponsorAsync(
            Operator,
            new SaveSponsorCommand("FitLife Gyms", "Amina", "amina@fitlife.test", null, "https://fitlife.test", true));

        Assert.Equal("FitLife Gyms", created.Name);
        Assert.True(created.IsActive);
        Assert.Equal(0, created.Offers);
        Assert.Single(harness.Sponsors.Items);
    }

    [Fact]
    public async Task A_reward_belongs_to_a_sponsor_and_zero_stock_is_out_of_stock()
    {
        var harness = new Harness();
        var sponsor = await harness.Sut.CreateSponsorAsync(
            Operator,
            new SaveSponsorCommand("FitLife Gyms", null, null, null, null, true));

        var reward = await harness.Sut.CreateRewardAsync(
            Operator,
            new SaveRewardCommand(sponsor.Id, "Gym day pass", "One visit", 4000, 0, RewardStatus.Active, null, null, null));

        Assert.Equal(RewardStatus.OutOfStock, reward.Status);
        Assert.Equal("FitLife Gyms", reward.SponsorName);
        Assert.Equal(4000, reward.PointsCost);
    }

    [Fact]
    public async Task A_reward_cannot_be_created_without_a_sponsor()
    {
        var harness = new Harness();

        var error = await Assert.ThrowsAsync<AdminException>(() => harness.Sut.CreateRewardAsync(
            Operator,
            new SaveRewardCommand(Guid.NewGuid(), "Voucher", null, 500, 10, RewardStatus.Active, null, null, null)));

        Assert.Equal(ErrorCodes.NotFound, error.Code);
        Assert.Empty(harness.Rewards.Items);
    }

    [Fact]
    public async Task Pausing_a_sponsor_is_audited()
    {
        var harness = new Harness();
        var sponsor = await harness.Sut.CreateSponsorAsync(
            Operator,
            new SaveSponsorCommand("FitLife Gyms", null, null, null, null, true));

        var updated = await harness.Sut.UpdateSponsorAsync(
            Operator,
            sponsor.Id,
            new SaveSponsorCommand("FitLife Gyms", null, null, null, null, false));

        Assert.False(updated.IsActive);
        Assert.Contains(harness.Audit.Items, e => e.ActionType == AuditActionType.SponsorModified && e.NewValue == "FitLife Gyms, paused");
    }

    private sealed class Harness
    {
        public Harness()
        {
            var time = new FixedTimeProvider(Now);
            Sut = new AdminMarketplaceService(
                Sponsors,
                Rewards,
                Redemptions,
                new AuditTrail(Audit, new FakeCallerContext("203.0.113.7"), time),
                UnitOfWork,
                time);
        }

        public FakeSponsorRepository Sponsors { get; } = new();
        public FakeRewardRepository Rewards { get; } = new();
        public FakeRewardRedemptionRepository Redemptions { get; } = new();
        public FakeAuditLogRepository Audit { get; } = new();
        public FakeUnitOfWork UnitOfWork { get; } = new();
        public AdminMarketplaceService Sut { get; }
    }
}
