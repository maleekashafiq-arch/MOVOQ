using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IProcessedCommandRepository
{
    void Add(ProcessedCommand processedCommand);

    Task<bool> ExistsByCommandIdAsync(Guid commandId, CancellationToken cancellationToken = default);
}
