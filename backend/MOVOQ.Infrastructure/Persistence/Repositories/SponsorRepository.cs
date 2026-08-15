using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class SponsorRepository(MovoqDbContext db) : ISponsorRepository
{
    public void Add(Sponsor sponsor) => db.Sponsors.Add(sponsor);

    public Task<Sponsor?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Sponsors.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Sponsor>> ListAsync(CancellationToken cancellationToken = default) =>
        await db.Sponsors
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
}
