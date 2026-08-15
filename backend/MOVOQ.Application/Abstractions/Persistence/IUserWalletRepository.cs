using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IUserWalletRepository
{
    void Add(UserWallet wallet);
}
