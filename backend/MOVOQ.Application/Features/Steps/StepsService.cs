using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Application.Features.Admin;
using MOVOQ.Application.Features.Identity;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Steps;

/// <summary>
/// Turns untrusted device step claims into a server verdict, and posts goal points to the ledger exactly once
/// per day. The ledger write and the wallet update always happen in the same transaction.
/// </summary>
public sealed class StepsService(
    IUserProvisioning users,
    IDailyStepRecordRepository stepRecords,
    IUserDeviceRepository devices,
    IUserWalletRepository wallets,
    IPointsLedgerRepository ledger,
    IProcessedCommandRepository processedCommands,
    IFraudSignalRepository fraudSignals,
    IPlatformSettingsService settings,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IStepsService
{
    private const string SyncCommandType = "steps.sync";

    public async Task<StepsSyncResultDto> SyncAsync(
        ExternalIdentity identity,
        SyncStepsCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(now);
        ValidateCommand(command, today);

        var user = await users.ResolveUserAsync(identity, cancellationToken);

        // A replayed command must never credit points a second time, so answer from stored state instead.
        if (await processedCommands.ExistsByCommandIdAsync(command.CommandId, cancellationToken))
        {
            var replay = await ReadStateAsync(user, command.StepDate, today, cancellationToken);
            return replay with { Duplicate = true };
        }

        var record = await stepRecords.GetByUserAndDateAsync(user.Id, command.StepDate, cancellationToken);
        if (record is null)
        {
            record = new DailyStepRecord
            {
                UserId = user.Id,
                User = user,
                StepDate = command.StepDate,
                Source = DataSource.Mobile,
            };
            stepRecords.Add(record);
        }

        await ApplyClaimAsync(user, record, command, now, cancellationToken);

        var reward = await settings.GetIntAsync(
            PlatformSettingKeys.DailyGoalReward,
            StepRewardOptions.GoalCompletionPoints,
            cancellationToken);
        var credited = AwardGoalPointsIfDue(user, record, reward, now);

        processedCommands.Add(new ProcessedCommand
        {
            CommandId = command.CommandId,
            UserId = user.Id,
            DeviceId = record.UserDeviceId,
            CommandType = SyncCommandType,
            Status = "Applied",
            ProcessedAt = now,
            RelatedEntityName = nameof(DailyStepRecord),
            RelatedEntityId = record.Id,
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var streak = await CalculateStreakAsync(user.Id, today, cancellationToken);
        return new StepsSyncResultDto(
            Map(record),
            credited,
            user.Wallet?.Balance ?? 0,
            streak,
            Duplicate: false);
    }

    public async Task<StepsSyncResultDto> GetTodayAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var user = await users.ResolveUserAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await ReadStateAsync(user, today, today, cancellationToken);
    }

    public async Task<StepsHistoryDto> GetHistoryAsync(
        ExternalIdentity identity,
        int days,
        CancellationToken cancellationToken = default)
    {
        var window = Math.Clamp(days, 1, StepRewardOptions.MaxHistoryDays);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var user = await users.ResolveUserAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var goal = ActiveGoal(user);
        var records = await stepRecords.ListRangeAsync(
            user.Id,
            today.AddDays(-(window - 1)),
            today,
            cancellationToken);

        var byDate = records.ToDictionary(r => r.StepDate);
        var timeline = new List<DailyStepsDto>(window);
        for (var offset = window - 1; offset >= 0; offset--)
        {
            var date = today.AddDays(-offset);
            timeline.Add(byDate.TryGetValue(date, out var record)
                ? Map(record)
                : new DailyStepsDto(date, 0, goal, false, 0, ValidationStatus.Pending));
        }

        return new StepsHistoryDto(
            timeline,
            timeline.Sum(d => d.Steps),
            timeline.Sum(d => d.PointsAwarded),
            await CalculateStreakAsync(user.Id, today, cancellationToken));
    }

    private async Task ApplyClaimAsync(
        User user,
        DailyStepRecord record,
        SyncStepsCommand command,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // Within a day a pedometer only counts up. A lower claim means a reboot or a second device,
        // so keep the high-water mark rather than letting the day regress.
        var claimed = Math.Max(record.ClaimedStepCount, command.StepCount);
        var implausible = claimed > StepRewardOptions.MaxPlausibleDailySteps;

        record.ClaimedStepCount = claimed;
        record.DeviceRecordedAt = command.DeviceRecordedAt ?? now;
        record.CommandId = command.CommandId;
        record.Source = DataSource.Mobile;
        record.SyncedAt = now;
        record.UpdatedAt = now;

        if (!string.IsNullOrWhiteSpace(command.DeviceIdentifier))
        {
            var device = await devices.GetByUserAndIdentifierAsync(
                user.Id,
                command.DeviceIdentifier.Trim(),
                cancellationToken);
            record.UserDeviceId = device?.Id ?? record.UserDeviceId;
            record.DeviceId = device?.Id ?? record.DeviceId;
        }

        var alreadyRejected = record.ValidationStatus == ValidationStatus.Rejected;

        // The goal is frozen on the first sync of a day, so changing the target later cannot retroactively
        // complete or un-complete a day that is already in progress.
        record.DailyTargetStepsSnapshot ??= ActiveGoal(user);
        record.ValidatedStepCount = Math.Min(claimed, StepRewardOptions.MaxPlausibleDailySteps);
        record.ValidationStatus = implausible ? ValidationStatus.Rejected : ValidationStatus.Validated;
        record.ValidatedAt = now;
        record.TargetCompleted = !implausible && record.ValidatedStepCount >= record.DailyTargetStepsSnapshot;
        record.PointsEligible = record.TargetCompleted;

        // Raise the signal once per day, not on every retry of an already-rejected day.
        if (implausible && !alreadyRejected)
        {
            fraudSignals.Add(new FraudSignal
            {
                UserId = user.Id,
                User = user,
                SignalType = FraudSignalType.AbnormalStepCount,
                Severity = FraudSignalSeverity.High,
                Description =
                    $"Claimed {claimed:N0} steps on {record.StepDate:yyyy-MM-dd}, above the plausible daily maximum of {StepRewardOptions.MaxPlausibleDailySteps:N0}.",
                ReferenceType = nameof(DailyStepRecord),
                ReferenceId = record.Id,
                DetectedAt = now,
                CreatedAt = now,
            });
        }
    }

    private long AwardGoalPointsIfDue(User user, DailyStepRecord record, int reward, DateTime now)
    {
        // A zero reward is a valid operator setting; skip the ledger entirely rather than writing noise.
        if (!record.PointsEligible || record.PointsAwarded || reward <= 0)
        {
            return 0;
        }

        var entry = new PointsLedger
        {
            UserId = user.Id,
            User = user,
            Amount = reward,
            TransactionType = PointsTransactionType.StepsEarned,
            ReferenceType = LedgerReferenceType.DailyStepRecord,
            ReferenceId = record.Id,
            Description = $"Daily goal of {record.DailyTargetStepsSnapshot} steps reached on {record.StepDate:yyyy-MM-dd}.",
            OccurredAt = now,
        };
        ledger.Add(entry);

        record.PointsAwarded = true;
        record.PointsAmount = entry.Amount;
        record.PointsLedger = entry;

        var wallet = user.Wallet;
        if (wallet is null)
        {
            // Explicit insert: a wallet reached only through the loaded user would be tracked as an update.
            wallet = new UserWallet { UserId = user.Id, User = user };
            wallets.Add(wallet);
            user.Wallet = wallet;
        }

        wallet.Balance += entry.Amount;
        wallet.LifetimeEarnedPoints += entry.Amount;
        wallet.UpdatedAt = now;

        return entry.Amount;
    }

    private async Task<StepsSyncResultDto> ReadStateAsync(
        User user,
        DateOnly stepDate,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        var record = await stepRecords.GetByUserAndDateAsync(user.Id, stepDate, cancellationToken);
        var day = record is null
            ? new DailyStepsDto(stepDate, 0, ActiveGoal(user), false, 0, ValidationStatus.Pending)
            : Map(record);

        return new StepsSyncResultDto(
            day,
            PointsCredited: 0,
            user.Wallet?.Balance ?? 0,
            await CalculateStreakAsync(user.Id, today, cancellationToken),
            Duplicate: false);
    }

    private async Task<int> CalculateStreakAsync(Guid userId, DateOnly today, CancellationToken cancellationToken)
    {
        var records = await stepRecords.ListRangeAsync(
            userId,
            today.AddDays(-StepRewardOptions.StreakWindowDays),
            today,
            cancellationToken);

        var completed = records
            .Where(r => r.TargetCompleted)
            .Select(r => r.StepDate)
            .ToHashSet();

        // Today is still in progress, so an incomplete today does not end the streak yet.
        var cursor = completed.Contains(today) ? today : today.AddDays(-1);
        var streak = 0;
        while (completed.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    private static void ValidateCommand(SyncStepsCommand command, DateOnly today)
    {
        if (command.CommandId == Guid.Empty)
        {
            throw new AppException(ErrorCodes.ValidationFailed, "CommandId is required.");
        }

        if (command.StepCount < 0)
        {
            throw new AppException(ErrorCodes.ValidationFailed, "Step count cannot be negative.");
        }

        // Device-local dates can run a day ahead of UTC, so allow tomorrow but nothing beyond it.
        if (command.StepDate > today.AddDays(1))
        {
            throw new AppException(ErrorCodes.ValidationFailed, "Step date cannot be in the future.");
        }

        if (command.StepDate < today.AddDays(-StepRewardOptions.LateSyncWindowDays))
        {
            throw new AppException(
                ErrorCodes.SyncWindowClosed,
                $"Steps can only be synced within {StepRewardOptions.LateSyncWindowDays} days of the day they were taken.",
                409);
        }
    }

    private static int ActiveGoal(User user) =>
        user.StepGoals.FirstOrDefault(g => g.IsActive)?.DailyTargetSteps ?? DailyGoalOptions.Default;

    private static DailyStepsDto Map(DailyStepRecord record) => new(
        record.StepDate,
        record.ValidatedStepCount ?? record.ClaimedStepCount,
        record.DailyTargetStepsSnapshot ?? DailyGoalOptions.Default,
        record.TargetCompleted,
        record.PointsAmount,
        record.ValidationStatus);
}
