using Microsoft.EntityFrameworkCore;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Repositories;

internal sealed class DailyStepRecordRepository(MovoqDbContext db) : IDailyStepRecordRepository
{
    public void Add(DailyStepRecord record) => db.DailyStepRecords.Add(record);

    public Task<DailyStepRecord?> GetByUserAndDateAsync(
        Guid userId,
        DateOnly stepDate,
        CancellationToken cancellationToken = default) =>
        db.DailyStepRecords.SingleOrDefaultAsync(
            x => x.UserId == userId && x.StepDate == stepDate,
            cancellationToken);

    public async Task<IReadOnlyList<DailyStepRecord>> ListRangeAsync(
        Guid userId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default) =>
        await db.DailyStepRecords
            .Where(x => x.UserId == userId && x.StepDate >= fromDate && x.StepDate <= toDate)
            .OrderBy(x => x.StepDate)
            .ToListAsync(cancellationToken);
}
