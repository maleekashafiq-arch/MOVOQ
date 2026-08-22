using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Shared;
using Npgsql;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class EfUnitOfWork(MovoqDbContext db) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AppException(
                ErrorCodes.ConcurrencyConflict,
                "That record changed in another request. Retry the action.",
                409);
        }
        catch (DbUpdateException ex) when (IsProcessedCommandDuplicate(ex))
        {
            throw new DuplicateCommandException();
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        if (db.Database.CurrentTransaction is not null)
        {
            return await action(cancellationToken);
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            if (transaction.GetDbTransaction().Connection is not null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            throw;
        }
    }

    private static bool IsProcessedCommandDuplicate(DbUpdateException ex) =>
        ex.InnerException is PostgresException postgres
        && postgres.SqlState == PostgresErrorCodes.UniqueViolation
        && postgres.ConstraintName is string name
        && name.Contains("processed_commands", StringComparison.OrdinalIgnoreCase)
        && name.Contains("CommandId", StringComparison.OrdinalIgnoreCase);
}