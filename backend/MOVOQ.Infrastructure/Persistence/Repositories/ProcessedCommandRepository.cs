using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class ProcessedCommandRepository(MovoqDbContext db) : IProcessedCommandRepository
{
    public void Add(ProcessedCommand processedCommand) => db.ProcessedCommands.Add(processedCommand);

    public Task<bool> ExistsByCommandIdAsync(Guid commandId, CancellationToken cancellationToken = default) =>
        db.ProcessedCommands.AsNoTracking().AnyAsync(x => x.CommandId == commandId, cancellationToken);
}
