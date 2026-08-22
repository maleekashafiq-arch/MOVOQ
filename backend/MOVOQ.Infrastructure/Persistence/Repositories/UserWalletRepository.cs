using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class UserWalletRepository(MovoqDbContext db) : IUserWalletRepository
{
    public void Add(UserWallet wallet) => db.UserWallets.Add(wallet);

    public async Task LockByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Holds the row until the ambient transaction commits. Zero rows is fine (wallet not created yet).
        await db.Database.ExecuteSqlAsync(
            $"""SELECT 1 FROM user_wallets WHERE "UserId" = {userId} AND "DeletedAt" IS NULL FOR UPDATE""",
            cancellationToken);

        // The user graph may have been loaded before this transaction. Refresh so we debit the
        // committed balance, not a stale in-memory copy from a concurrent request.
        var tracked = db.ChangeTracker.Entries<UserWallet>()
            .FirstOrDefault(e => e.Entity.UserId == userId && e.Entity.DeletedAt is null);
        if (tracked is not null)
        {
            await tracked.ReloadAsync(cancellationToken);
        }
    }
}