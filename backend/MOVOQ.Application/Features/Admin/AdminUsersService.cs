using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

/// <summary>
/// Back-office view of end users, plus the two privileged writes operators need: correcting a balance and
/// changing an account's standing. Both post through the same ledger and status rules the app obeys, so
/// support action and user-earned activity stay reconcilable.
/// </summary>
public sealed class AdminUsersService(
    IAdminReportingRepository reporting,
    IUserRepository users,
    IUserWalletRepository wallets,
    IPointsLedgerRepository ledger,
    IDailyStepRecordRepository stepRecords,
    IAuditTrail audit,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IAdminUsersService
{
    private const int MaxPageSize = 100;
    private const int MaxHistoryDays = 90;
    private const int RecentLedgerEntries = 20;

    public async Task<AdminUserListDto> ListAsync(
        string? query,
        UserStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var safePage = Math.Max(page, 1);
        var safeSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var results = await reporting.SearchUsersAsync(
            string.IsNullOrWhiteSpace(query) ? null : query.Trim(),
            status,
            safePage,
            safeSize,
            cancellationToken);

        return new AdminUserListDto(
            results.Items.Select(Map).ToList(),
            results.TotalCount,
            safePage,
            safeSize,
            await reporting.CountUsersByStatusAsync(UserStatus.Active, cancellationToken),
            await reporting.CountUsersByStatusAsync(UserStatus.Suspended, cancellationToken),
            await reporting.CountUsersByStatusAsync(UserStatus.PendingVerification, cancellationToken));
    }

    public async Task<AdminUserDetailDto> GetAsync(
        Guid userId,
        int historyDays,
        CancellationToken cancellationToken = default)
    {
        var row = await reporting.GetUserRowAsync(userId, cancellationToken)
            ?? throw NotFound(userId);

        var window = Math.Clamp(historyDays, 1, MaxHistoryDays);
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var records = await stepRecords.ListRangeAsync(
            userId,
            today.AddDays(-(window - 1)),
            today,
            cancellationToken);

        var byDate = records.ToDictionary(r => r.StepDate);
        var history = new List<AdminUserStepDayDto>(window);
        for (var offset = window - 1; offset >= 0; offset--)
        {
            var date = today.AddDays(-offset);
            history.Add(byDate.TryGetValue(date, out var record)
                ? new AdminUserStepDayDto(
                    date,
                    record.ValidatedStepCount ?? record.ClaimedStepCount,
                    record.DailyTargetStepsSnapshot ?? row.DailyTargetSteps,
                    record.TargetCompleted,
                    record.PointsAmount)
                : new AdminUserStepDayDto(date, 0, row.DailyTargetSteps, false, 0));
        }

        var entries = await reporting.GetUserLedgerAsync(userId, RecentLedgerEntries, cancellationToken);
        return new AdminUserDetailDto(
            Map(row),
            history,
            entries.Select(e => new AdminLedgerEntryDto(e.Id, e.Amount, e.TransactionType, e.Description, e.OccurredAt)).ToList());
    }

    public async Task<AdjustPointsResultDto> AdjustPointsAsync(
        AdminContext admin,
        Guid userId,
        AdjustPointsCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.Amount == 0)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Enter a non-zero adjustment.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "A reason is required for every manual adjustment.");
        }

        var user = await users.GetByIdForIdentityAsync(userId, cancellationToken) ?? throw NotFound(userId);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        return await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await wallets.LockByUserIdAsync(user.Id, ct);
                return await AdjustLockedWalletAsync(admin, user, command, now, ct);
            },
            cancellationToken);
    }

    private async Task<AdjustPointsResultDto> AdjustLockedWalletAsync(
        AdminContext admin,
        User user,
        AdjustPointsCommand command,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var wallet = user.Wallet;
        if (wallet is null)
        {
            // Explicit insert: a wallet reached only through the loaded user would be tracked as an update.
            wallet = new UserWallet { UserId = user.Id, User = user };
            wallets.Add(wallet);
            user.Wallet = wallet;
        }

        var before = wallet.Balance;
        if (before + command.Amount < 0)
        {
            throw new AdminException(
                ErrorCodes.ValidationFailed,
                $"Deducting {Math.Abs(command.Amount)} points would take the balance below zero (currently {before}).");
        }

        var reason = command.Reason.Trim();
        ledger.Add(new PointsLedger
        {
            UserId = user.Id,
            User = user,
            Amount = command.Amount,
            TransactionType = PointsTransactionType.AdminAdjustment,
            ReferenceType = LedgerReferenceType.AdminAdjustment,
            ReferenceId = admin.AdminUserId,
            Description = $"{reason} (by {admin.Email})",
            OccurredAt = now,
        });

        wallet.Balance = before + command.Amount;
        if (command.Amount > 0)
        {
            wallet.LifetimeEarnedPoints += command.Amount;
        }
        else
        {
            wallet.LifetimeSpentPoints += -command.Amount;
        }

        wallet.UpdatedAt = now;

        audit.Record(
            admin,
            AuditActionType.PointsAdjusted,
            nameof(UserWallet),
            user.Id,
            before.ToString(),
            $"{wallet.Balance} ({command.Amount:+#;-#;0}): {reason}");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new AdjustPointsResultDto(user.Id, command.Amount, wallet.Balance);
    }

    public async Task<AdminUserDto> ChangeStatusAsync(
        AdminContext admin,
        Guid userId,
        ChangeUserStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(command.Status))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Unknown account status.");
        }

        var user = await users.GetByIdForIdentityAsync(userId, cancellationToken) ?? throw NotFound(userId);
        var before = user.Status;
        if (before == command.Status)
        {
            return Map(await reporting.GetUserRowAsync(userId, cancellationToken) ?? throw NotFound(userId));
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        user.Status = command.Status;
        user.UpdatedAt = now;

        var reason = string.IsNullOrWhiteSpace(command.Reason) ? null : command.Reason.Trim();
        audit.Record(
            admin,
            AuditActionType.UserStatusChanged,
            nameof(User),
            user.Id,
            before.ToString(),
            reason is null ? command.Status.ToString() : $"{command.Status}: {reason}");

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var row = await reporting.GetUserRowAsync(userId, cancellationToken);
        return row is null
            ? new AdminUserDto(
                user.Id,
                user.Email,
                DisplayName(user.Profile?.DisplayName, user.Email),
                user.PhoneNumber,
                user.Profile?.City,
                user.Profile?.Country,
                user.Profile?.Gender,
                user.StepGoals.FirstOrDefault(g => g.IsActive)?.DailyTargetSteps ?? DailyGoalOptions.Default,
                user.Wallet?.Balance ?? 0,
                0,
                0,
                user.Status,
                user.CreatedAt,
                user.LastLoginAt)
            : Map(row);
    }

    private static AdminUserDto Map(AdminUserRow row) => new(
        row.Id,
        row.Email,
        DisplayName(row.DisplayName, row.Email),
        row.PhoneNumber,
        row.City,
        row.Country,
        row.Gender,
        row.DailyTargetSteps,
        row.PointsBalance,
        row.LifetimeSteps,
        row.ActiveTickets,
        row.Status,
        row.CreatedAt,
        row.LastLoginAt);

    /// <summary>Users who skipped profile completion still need a label in the table.</summary>
    private static string DisplayName(string? displayName, string email)
    {
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        var at = email.IndexOf('@');
        return at > 0 ? email[..at] : email;
    }

    private static AdminException NotFound(Guid userId) =>
        new(ErrorCodes.NotFound, $"No user with id {userId}.", 404);
}
