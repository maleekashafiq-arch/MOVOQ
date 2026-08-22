using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Application.Features.Identity;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Marketplace;

/// <summary>
/// User-facing catalogue and redemption. Points leave the wallet in the same transaction as the
/// redemption row, so every voucher can be traced to a ledger debit.
/// </summary>
public sealed class MarketplaceService(
    IUserProvisioning users,
    IRewardRepository rewards,
    IRewardRedemptionRepository redemptions,
    IUserWalletRepository wallets,
    IPointsLedgerRepository ledger,
    IProcessedCommandRepository processedCommands,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IMarketplaceService
{
    private const string RedeemCommandType = "rewards.redeem";

    public async Task<MarketplaceListDto> ListAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var user = await users.ResolveUserAsync(identity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var catalogue = await rewards.ListAsync(cancellationToken);
        var mine = await redemptions.ListForUserAsync(user.Id, cancellationToken);

        return new MarketplaceListDto(
            catalogue.Where(r => IsOnSale(r, now)).Select(MapReward).ToList(),
            mine.Select(MapRedemption).ToList(),
            user.Wallet?.Balance ?? 0);
    }

    public async Task<RedeemResultDto> RedeemAsync(
        ExternalIdentity identity,
        Guid rewardId,
        RedeemCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.CommandId == Guid.Empty)
        {
            throw new AppException(ErrorCodes.ValidationFailed, "CommandId is required.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var user = await users.ResolveUserAsync(identity, cancellationToken);

        if (await processedCommands.ExistsByCommandIdAsync(command.CommandId, cancellationToken))
        {
            return await ReadStateAsync(user, rewardId, cancellationToken) with { Duplicate = true };
        }

        try
        {
            return await unitOfWork.ExecuteInTransactionAsync(
                ct => ApplyRedeemAsync(user, rewardId, command, now, ct),
                cancellationToken);
        }
        catch (DuplicateCommandException)
        {
            return await ReadStateAsync(user, rewardId, cancellationToken) with { Duplicate = true };
        }
    }

    private async Task<RedeemResultDto> ApplyRedeemAsync(
        User user,
        Guid rewardId,
        RedeemCommand command,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await wallets.LockByUserIdAsync(user.Id, cancellationToken);

        var reward = await rewards.GetForUpdateAsync(rewardId, cancellationToken)
            ?? throw new AppException(ErrorCodes.NotFound, "That reward is no longer listed.", 404);

        if (!IsOnSale(reward, now))
        {
            throw new AppException(ErrorCodes.RewardUnavailable, "That reward is not available right now.", 409);
        }

        if (reward.StockQuantity is 0)
        {
            throw new AppException(ErrorCodes.OutOfStock, "That reward is out of stock.", 409);
        }

        var wallet = EnsureWallet(user);
        if (wallet.Balance < reward.PointsCost)
        {
            throw new AppException(
                ErrorCodes.InsufficientPoints,
                $"That costs {reward.PointsCost} points and your balance is {wallet.Balance}.",
                409);
        }

        if (reward.StockQuantity is int stock)
        {
            reward.StockQuantity = stock - 1;
            if (reward.StockQuantity == 0)
            {
                reward.Status = RewardStatus.OutOfStock;
            }
            reward.UpdatedAt = now;
        }

        var redemption = new RewardRedemption
        {
            UserId = user.Id,
            User = user,
            RewardId = reward.Id,
            Reward = reward,
            Status = RedemptionStatus.Confirmed,
            PointsCostSnapshot = reward.PointsCost,
            AssignedCode = NextCode(),
            RequestedAt = now,
            ConfirmedAt = now,
            CreatedAt = now,
        };

        if (reward.PointsCost > 0)
        {
            var entry = new PointsLedger
            {
                UserId = user.Id,
                User = user,
                Amount = -reward.PointsCost,
                TransactionType = PointsTransactionType.RedemptionDeducted,
                ReferenceType = LedgerReferenceType.RewardRedemption,
                ReferenceId = redemption.Id,
                Description = $"Redeemed {reward.Name}.",
                OccurredAt = now,
            };
            ledger.Add(entry);
            redemption.PointsLedger = entry;
            wallet.Balance -= reward.PointsCost;
            wallet.LifetimeSpentPoints += reward.PointsCost;
            wallet.UpdatedAt = now;
        }

        redemptions.Add(redemption);
        processedCommands.Add(new ProcessedCommand
        {
            CommandId = command.CommandId,
            UserId = user.Id,
            CommandType = RedeemCommandType,
            Status = "Applied",
            ProcessedAt = now,
            RelatedEntityName = nameof(RewardRedemption),
            RelatedEntityId = redemption.Id,
        });

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RedeemResultDto(
            redemption.Id,
            reward.Name,
            redemption.AssignedCode,
            reward.PointsCost,
            wallet.Balance,
            redemption.Status.ToString(),
            Duplicate: false);
    }

    private async Task<RedeemResultDto> ReadStateAsync(
        User user,
        Guid rewardId,
        CancellationToken cancellationToken)
    {
        var mine = await redemptions.ListForUserAsync(user.Id, cancellationToken);
        var last = mine.FirstOrDefault(r => r.RewardId == rewardId);

        return new RedeemResultDto(
            last?.Id ?? Guid.Empty,
            last?.Reward.Name ?? "Reward",
            last?.AssignedCode,
            PointsSpent: 0,
            user.Wallet?.Balance ?? 0,
            last?.Status.ToString() ?? RedemptionStatus.Confirmed.ToString(),
            Duplicate: false);
    }

    private UserWallet EnsureWallet(User user)
    {
        if (user.Wallet is not null)
        {
            return user.Wallet;
        }

        var wallet = new UserWallet { UserId = user.Id, User = user };
        wallets.Add(wallet);
        user.Wallet = wallet;
        return wallet;
    }

    private static bool IsOnSale(Reward reward, DateTime now)
    {
        if (reward.Status != RewardStatus.Active)
        {
            return false;
        }

        if (reward.Sponsor is { IsActive: false })
        {
            return false;
        }

        if (reward.ValidFrom is DateTime from && from > now)
        {
            return false;
        }

        if (reward.ValidUntil is DateTime until && until < now)
        {
            return false;
        }

        return true;
    }

    private static MarketplaceRewardDto MapReward(Reward reward) => new(
        reward.Id,
        reward.Name,
        reward.Description,
        reward.Sponsor?.Name ?? "Sponsor",
        reward.PointsCost,
        reward.StockQuantity,
        reward.ValidUntil,
        reward.TermsAndConditions);

    private static MyRedemptionDto MapRedemption(RewardRedemption redemption) => new(
        redemption.Id,
        redemption.Reward.Name,
        redemption.Reward.Sponsor?.Name ?? "Sponsor",
        redemption.PointsCostSnapshot,
        redemption.Status.ToString(),
        redemption.AssignedCode,
        redemption.RequestedAt);

    private static string NextCode() => $"MVQ-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}
