using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IUserWalletRepository
{
    void Add(UserWallet wallet);

    /// <summary>
    /// Locks the wallet row until the current transaction commits. No-op if the user has no wallet yet.
    /// Must be called inside <see cref="IUnitOfWork.ExecuteInTransactionAsync{T}"/>.
    /// </summary>
    Task LockByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
