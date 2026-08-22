using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Application.Features.Admin;
using MOVOQ.Application.Features.Identity;
using MOVOQ.Application.Features.Steps;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Tests.Unit.Steps;

public sealed class StepsServiceTests
{
    private static readonly ExternalIdentity Identity = new("uid-1", "walker@movoq.test", true, "Walker");
    private static readonly DateTime Now = new(2026, 8, 15, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    [Fact]
    public async Task Reaching_the_goal_credits_points_once_and_moves_the_wallet()
    {
        var harness = new Harness(dailyGoal: 6000);

        var result = await harness.SyncAsync(Today, 6200);

        Assert.Equal(6200, result.Day.Steps);
        Assert.True(result.Day.GoalCompleted);
        Assert.Equal(StepRewardOptions.GoalCompletionPoints, result.PointsCredited);
        Assert.Equal(StepRewardOptions.GoalCompletionPoints, result.WalletBalance);
        Assert.Equal(1, result.CurrentStreak);
        Assert.Equal(ValidationStatus.Validated, result.Day.ValidationStatus);
        Assert.Single(harness.Ledger.Items);
        Assert.Equal(LedgerReferenceType.DailyStepRecord, harness.Ledger.Items[0].ReferenceType);
        Assert.Equal(StepRewardOptions.GoalCompletionPoints, harness.User.Wallet!.LifetimeEarnedPoints);
    }

    [Fact]
    public async Task Later_syncs_on_the_same_day_never_credit_the_goal_twice()
    {
        var harness = new Harness(dailyGoal: 6000);

        await harness.SyncAsync(Today, 6200);
        var second = await harness.SyncAsync(Today, 9400);

        Assert.Equal(9400, second.Day.Steps);
        Assert.Equal(0, second.PointsCredited);
        Assert.Equal(StepRewardOptions.GoalCompletionPoints, second.WalletBalance);
        Assert.Single(harness.Ledger.Items);
    }

    [Fact]
    public async Task Replaying_a_command_reports_the_stored_day_without_reapplying()
    {
        var harness = new Harness(dailyGoal: 6000);
        var commandId = Guid.NewGuid();

        await harness.SyncAsync(Today, 6200, commandId);
        var replay = await harness.SyncAsync(Today, 99_000, commandId);

        Assert.True(replay.Duplicate);
        Assert.Equal(6200, replay.Day.Steps);
        Assert.Equal(0, replay.PointsCredited);
        Assert.Single(harness.Ledger.Items);
        Assert.Single(harness.ProcessedCommands.Items);
    }

    [Fact]
    public async Task Steps_below_the_goal_earn_nothing()
    {
        var harness = new Harness(dailyGoal: 6000);

        var result = await harness.SyncAsync(Today, 3420);

        Assert.False(result.Day.GoalCompleted);
        Assert.Equal(0, result.PointsCredited);
        Assert.Equal(0, result.WalletBalance);
        Assert.Equal(0, result.CurrentStreak);
        Assert.Empty(harness.Ledger.Items);
    }

    [Fact]
    public async Task An_implausible_day_is_rejected_and_earns_nothing()
    {
        var harness = new Harness(dailyGoal: 6000);

        var result = await harness.SyncAsync(Today, StepRewardOptions.MaxPlausibleDailySteps + 1);

        Assert.Equal(ValidationStatus.Rejected, result.Day.ValidationStatus);
        Assert.False(result.Day.GoalCompleted);
        Assert.Equal(0, result.PointsCredited);
        Assert.Empty(harness.Ledger.Items);
    }

    [Fact]
    public async Task An_implausible_day_raises_one_fraud_signal_however_often_it_is_retried()
    {
        var harness = new Harness(dailyGoal: 6000);

        await harness.SyncAsync(Today, StepRewardOptions.MaxPlausibleDailySteps + 1);
        await harness.SyncAsync(Today, StepRewardOptions.MaxPlausibleDailySteps + 500);

        var signal = Assert.Single(harness.FraudSignals.Items);
        Assert.Equal(FraudSignalType.AbnormalStepCount, signal.SignalType);
        Assert.Equal(harness.User.Id, signal.UserId);
    }

    [Fact]
    public async Task The_goal_reward_follows_the_operator_setting()
    {
        var harness = new Harness(dailyGoal: 6000, goalReward: 120);

        var result = await harness.SyncAsync(Today, 6200);

        Assert.Equal(120, result.PointsCredited);
        Assert.Equal(120, result.WalletBalance);
    }

    [Fact]
    public async Task A_zero_reward_completes_the_goal_without_writing_to_the_ledger()
    {
        var harness = new Harness(dailyGoal: 6000, goalReward: 0);

        var result = await harness.SyncAsync(Today, 6200);

        Assert.True(result.Day.GoalCompleted);
        Assert.Equal(0, result.PointsCredited);
        Assert.Empty(harness.Ledger.Items);
    }

    [Fact]
    public async Task A_lower_claim_cannot_walk_the_day_backwards()
    {
        var harness = new Harness(dailyGoal: 6000);

        await harness.SyncAsync(Today, 6200);
        var afterReboot = await harness.SyncAsync(Today, 120);

        Assert.Equal(6200, afterReboot.Day.Steps);
        Assert.True(afterReboot.Day.GoalCompleted);
    }

    [Fact]
    public async Task Changing_the_goal_mid_day_does_not_rewrite_a_day_in_progress()
    {
        var harness = new Harness(dailyGoal: 6000);
        await harness.SyncAsync(Today, 4200);

        harness.SetDailyGoal(4000);
        var result = await harness.SyncAsync(Today, 4300);

        Assert.Equal(6000, result.Day.DailyTargetSteps);
        Assert.False(result.Day.GoalCompleted);
        Assert.Equal(0, result.PointsCredited);
    }

    [Fact]
    public async Task Days_outside_the_late_sync_window_are_refused_permanently()
    {
        var harness = new Harness(dailyGoal: 6000);

        var stale = await Assert.ThrowsAsync<AppException>(() =>
            harness.SyncAsync(Today.AddDays(-(StepRewardOptions.LateSyncWindowDays + 1)), 6200));
        Assert.Equal(ErrorCodes.SyncWindowClosed, stale.Code);
        Assert.Equal(409, stale.StatusCode);

        var future = await Assert.ThrowsAsync<AppException>(() => harness.SyncAsync(Today.AddDays(2), 6200));
        Assert.Equal(ErrorCodes.ValidationFailed, future.Code);
    }

    [Fact]
    public async Task A_device_local_day_ahead_of_utc_is_still_accepted()
    {
        var harness = new Harness(dailyGoal: 6000);

        var result = await harness.SyncAsync(Today.AddDays(1), 6200);

        Assert.Equal(Today.AddDays(1), result.Day.StepDate);
        Assert.Equal(StepRewardOptions.GoalCompletionPoints, result.PointsCredited);
    }

    [Fact]
    public async Task The_streak_counts_back_from_yesterday_while_today_is_unfinished()
    {
        var harness = new Harness(dailyGoal: 6000);
        await harness.SyncAsync(Today.AddDays(-3), 7000);
        await harness.SyncAsync(Today.AddDays(-2), 7000);
        await harness.SyncAsync(Today.AddDays(-1), 7000);

        var today = await harness.SyncAsync(Today, 1200);

        Assert.False(today.Day.GoalCompleted);
        Assert.Equal(3, today.CurrentStreak);
    }

    [Fact]
    public async Task A_missed_day_breaks_the_streak()
    {
        var harness = new Harness(dailyGoal: 6000);
        await harness.SyncAsync(Today.AddDays(-3), 7000);
        await harness.SyncAsync(Today.AddDays(-2), 500);

        var result = await harness.SyncAsync(Today.AddDays(-1), 7000);

        Assert.Equal(1, result.CurrentStreak);
    }

    [Fact]
    public async Task The_wallet_stays_reconciled_with_the_ledger()
    {
        var harness = new Harness(dailyGoal: 4000);
        await harness.SyncAsync(Today.AddDays(-2), 4200);
        await harness.SyncAsync(Today.AddDays(-1), 8000);
        await harness.SyncAsync(Today, 4000);

        var ledgerSum = await harness.Ledger.SumLifetimeAmountsAsync(harness.User.Id);

        Assert.Equal(3 * StepRewardOptions.GoalCompletionPoints, ledgerSum);
        Assert.Equal(ledgerSum, harness.User.Wallet!.Balance);
    }

    [Fact]
    public async Task History_fills_missing_days_with_zeroes()
    {
        var harness = new Harness(dailyGoal: 6000);
        await harness.SyncAsync(Today.AddDays(-2), 6500);
        await harness.SyncAsync(Today, 1000);

        var history = await harness.Sut.GetHistoryAsync(Identity, 3);

        Assert.Equal(3, history.Days.Count);
        Assert.Equal(new[] { 6500, 0, 1000 }, history.Days.Select(d => d.Steps).ToArray());
        Assert.Equal(7500, history.TotalSteps);
        Assert.Equal(StepRewardOptions.GoalCompletionPoints, history.TotalPointsAwarded);
        Assert.Equal(0, history.CurrentStreak);
    }

    [Fact]
    public async Task Today_reads_stored_state_without_writing_points()
    {
        var harness = new Harness(dailyGoal: 6000);
        await harness.SyncAsync(Today, 6100);

        var today = await harness.Sut.GetTodayAsync(Identity);

        Assert.Equal(6100, today.Day.Steps);
        Assert.Equal(0, today.PointsCredited);
        Assert.Equal(StepRewardOptions.GoalCompletionPoints, today.WalletBalance);
        Assert.Single(harness.Ledger.Items);
    }

    [Fact]
    public async Task Ten_thousand_steps_in_five_minutes_is_rejected()
    {
        var harness = new Harness(dailyGoal: 6000);
        await harness.SyncAsync(Today, 1_000, deviceRecordedAt: Now);

        var result = await harness.SyncAsync(
            Today,
            1_000 + StepRewardOptions.MaxStepsInFiveMinutes,
            deviceRecordedAt: Now.AddMinutes(5));

        Assert.Equal(ValidationStatus.Rejected, result.Day.ValidationStatus);
        Assert.Equal(0, result.PointsCredited);
        Assert.Empty(harness.Ledger.Items);
        var signal = Assert.Single(harness.FraudSignals.Items);
        Assert.Equal(FraudSignalType.ImpossibleStepRate, signal.SignalType);
    }

    [Fact]
    public async Task A_device_clock_ahead_of_the_server_is_rejected()
    {
        var harness = new Harness(dailyGoal: 6000);

        var result = await harness.SyncAsync(
            Today,
            6200,
            deviceRecordedAt: Now.AddMinutes(StepRewardOptions.DeviceClockSkewMinutes + 1));

        Assert.Equal(ValidationStatus.Rejected, result.Day.ValidationStatus);
        Assert.Equal(0, result.PointsCredited);
        Assert.Empty(harness.Ledger.Items);
        var signal = Assert.Single(harness.FraudSignals.Items);
        Assert.Equal(FraudSignalType.TamperedTimestamp, signal.SignalType);
    }

    private sealed class Harness
    {
        public Harness(int dailyGoal, int? goalReward = null)
        {
            User = new User { Email = Identity.Email! };
            User.Wallet = new UserWallet { UserId = User.Id, User = User };
            SetDailyGoal(dailyGoal);
            Settings = new FakePlatformSettings(goalReward);

            Sut = new StepsService(
                new FakeUserProvisioning(User),
                StepRecords,
                Devices,
                Wallets,
                Ledger,
                ProcessedCommands,
                FraudSignals,
                Settings,
                new FakeUnitOfWork(),
                new FixedTimeProvider(Now));
        }

        public User User { get; }

        public FakeStepRecordRepository StepRecords { get; } = new();

        public FakeDeviceRepository Devices { get; } = new();

        public FakeWalletRepository Wallets { get; } = new();

        public FakeLedgerRepository Ledger { get; } = new();

        public FakeProcessedCommandRepository ProcessedCommands { get; } = new();

        public FakeFraudSignalRepository FraudSignals { get; } = new();

        public FakePlatformSettings Settings { get; }

        public StepsService Sut { get; }

        public void SetDailyGoal(int dailyTargetSteps)
        {
            foreach (var goal in User.StepGoals)
            {
                goal.IsActive = false;
            }

            User.StepGoals.Add(new StepGoal
            {
                User = User,
                UserId = User.Id,
                DailyTargetSteps = dailyTargetSteps,
                EffectiveFrom = Now,
                IsActive = true,
            });
        }

        public Task<StepsSyncResultDto> SyncAsync(
            DateOnly date,
            int steps,
            Guid? commandId = null,
            DateTime? deviceRecordedAt = null) =>
            Sut.SyncAsync(
                Identity,
                new SyncStepsCommand(commandId ?? Guid.NewGuid(), date, steps, null, deviceRecordedAt));
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

    private sealed class FakeFraudSignalRepository : IFraudSignalRepository
    {
        public List<FraudSignal> Items { get; } = [];

        public void Add(FraudSignal signal) => Items.Add(signal);

        public Task<IReadOnlyList<FraudSignal>> ListRecentAsync(int take, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<FraudSignal>>(Items.Take(take).ToList());

        public Task<int> CountSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Count(x => x.DetectedAt >= sinceUtc));
    }

    /// <summary>Returns the compiled default unless a test overrides the goal reward.</summary>
    private sealed class FakePlatformSettings(int? goalReward) : IPlatformSettingsService
    {
        public Task<PlatformSettingsDto> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(PlatformSettingsService.Defaults);

        public Task<PlatformSettingsDto> UpdateAsync(
            AdminContext admin,
            PlatformSettingsDto settings,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<int> GetIntAsync(string key, int fallback, CancellationToken cancellationToken = default) =>
            Task.FromResult(key == PlatformSettingKeys.DailyGoalReward ? goalReward ?? fallback : fallback);
    }

    private sealed class FakeStepRecordRepository : IDailyStepRecordRepository
    {
        public List<DailyStepRecord> Items { get; } = [];

        public void Add(DailyStepRecord record) => Items.Add(record);

        public Task<DailyStepRecord?> GetByUserAndDateAsync(
            Guid userId,
            DateOnly stepDate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(x => x.UserId == userId && x.StepDate == stepDate));

        public Task<IReadOnlyList<DailyStepRecord>> ListRangeAsync(
            Guid userId,
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<DailyStepRecord>>(Items
                .Where(x => x.UserId == userId && x.StepDate >= fromDate && x.StepDate <= toDate)
                .OrderBy(x => x.StepDate)
                .ToList());
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
