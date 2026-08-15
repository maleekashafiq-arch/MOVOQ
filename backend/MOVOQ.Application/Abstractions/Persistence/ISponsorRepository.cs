using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface ISponsorRepository
{
    void Add(Sponsor sponsor);

    Task<Sponsor?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Sponsor>> ListAsync(CancellationToken cancellationToken = default);
}
