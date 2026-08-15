using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class UserWalletRepository(MovoqDbContext db) : IUserWalletRepository
{
    public void Add(UserWallet wallet) => db.UserWallets.Add(wallet);
}
