using MOVOQ.Application.Features.Admin;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;
using MOVOQ.Tests.Unit.Admin.Fakes;

namespace MOVOQ.Tests.Unit.Admin;

public sealed class PlatformSettingsServiceTests
{
    private static readonly DateTime Now = new(2026, 8, 15, 9, 0, 0, DateTimeKind.Utc);
    private static readonly AdminContext Operator = new(Guid.NewGuid(), "ops@movoq.test", "Ops", AdminRole.Admin);

    [Fact]
    public async Task An_empty_table_reads_back_as_the_compiled_defaults()
    {
        var harness = new Harness();

        var settings = await harness.Sut.GetAsync();

        Assert.Equal(PlatformSettingsService.Defaults, settings);
        Assert.Equal(StepRewardOptions.GoalCompletionPoints, settings.DailyGoalReward);
    }

    [Fact]
    public async Task A_stored_value_overrides_the_default()
    {
        var harness = new Harness();
        harness.Store(PlatformSettingKeys.DailyGoalReward, "120");
        harness.Store(PlatformSettingKeys.FeatureLuckyDraws, "false");

        var settings = await harness.Sut.GetAsync();

        Assert.Equal(120, settings.DailyGoalReward);
        Assert.False(settings.FeatureLuckyDraws);
        Assert.True(settings.FeatureTeams);
    }

    [Fact]
    public async Task A_corrupt_value_falls_back_rather_than_throwing()
    {
        var harness = new Harness();
        harness.Store(PlatformSettingKeys.DailyGoalReward, "not-a-number");

        var settings = await harness.Sut.GetAsync();

        Assert.Equal(PlatformSettingsService.Defaults.DailyGoalReward, settings.DailyGoalReward);
    }

    [Fact]
    public async Task Saving_writes_only_the_keys_that_changed_and_audits_the_diff()
    {
        var harness = new Harness();

        var saved = await harness.Sut.UpdateAsync(
            Operator,
            PlatformSettingsService.Defaults with { DailyGoalReward = 75, CoachPrice = 349 });

        Assert.Equal(75, saved.DailyGoalReward);
        Assert.Equal(349, saved.CoachPrice);
        Assert.Equal(2, harness.Settings.Items.Count);

        var entry = Assert.Single(harness.Audit.Items);
        Assert.Equal(AuditActionType.SettingsUpdated, entry.ActionType);
        Assert.Contains("points.dailyGoalReward=50", entry.OldValue);
        Assert.Contains("points.dailyGoalReward=75", entry.NewValue);
        Assert.Contains("aiCoach.price=349", entry.NewValue);
        Assert.DoesNotContain("app.name", entry.NewValue);
    }

    [Fact]
    public async Task Saving_an_unchanged_payload_writes_nothing()
    {
        var harness = new Harness();

        await harness.Sut.UpdateAsync(Operator, PlatformSettingsService.Defaults);

        Assert.Empty(harness.Settings.Items);
        Assert.Empty(harness.Audit.Items);
        Assert.Equal(0, harness.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Saving_twice_updates_the_existing_row_rather_than_adding_another()
    {
        var harness = new Harness();

        await harness.Sut.UpdateAsync(Operator, PlatformSettingsService.Defaults with { DailyGoalReward = 75 });
        await harness.Sut.UpdateAsync(Operator, PlatformSettingsService.Defaults with { DailyGoalReward = 90 });

        var row = Assert.Single(harness.Settings.Items);
        Assert.Equal("90", row.Value);
        Assert.Equal(Operator.AdminUserId, row.LastModifiedBy);
    }

    [Theory]
    [InlineData(-1, 100, "support@movoq.app")]
    [InlineData(50, 0, "support@movoq.app")]
    [InlineData(50, 100, "not-an-email")]
    public async Task Invalid_settings_are_rejected_before_anything_is_written(
        int goalReward,
        int conversionRate,
        string supportEmail)
    {
        var harness = new Harness();

        var error = await Assert.ThrowsAsync<AdminException>(() => harness.Sut.UpdateAsync(
            Operator,
            PlatformSettingsService.Defaults with
            {
                DailyGoalReward = goalReward,
                TicketConversionRate = conversionRate,
                SupportEmail = supportEmail,
            }));

        Assert.Equal(ErrorCodes.ValidationFailed, error.Code);
        Assert.Empty(harness.Settings.Items);
    }

    [Fact]
    public async Task A_single_value_can_be_read_for_runtime_use()
    {
        var harness = new Harness();
        harness.Store(PlatformSettingKeys.AdReward, "25");

        Assert.Equal(25, await harness.Sut.GetIntAsync(PlatformSettingKeys.AdReward, 10));
        Assert.Equal(7, await harness.Sut.GetIntAsync("never.saved", 7));
    }

    private sealed class Harness
    {
        public Harness()
        {
            var time = new FixedTimeProvider(Now);
            Sut = new PlatformSettingsService(
                Settings,
                new AuditTrail(Audit, new FakeCallerContext(null), time),
                UnitOfWork,
                time);
        }

        public FakeAppSettingRepository Settings { get; } = new();

        public FakeAuditLogRepository Audit { get; } = new();

        public FakeUnitOfWork UnitOfWork { get; } = new();

        public PlatformSettingsService Sut { get; }

        public void Store(string key, string value) =>
            Settings.Add(new AppSetting { Key = key, Value = value });
    }
}
