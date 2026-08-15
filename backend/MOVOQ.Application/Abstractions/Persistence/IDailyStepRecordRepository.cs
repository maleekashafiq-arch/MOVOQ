using MOVOQ.Domain.Entities;

namespace MOVOQ.Application.Abstractions.Persistence;

public interface IDailyStepRecordRepository
{
    void Add(DailyStepRecord record);

    Task<DailyStepRecord?> GetByUserAndDateAsync(
        Guid userId,
        DateOnly stepDate,
        CancellationToken cancellationToken = default);

    /// <summary>Tracked records for an inclusive date range, oldest first.</summary>
    Task<IReadOnlyList<DailyStepRecord>> ListRangeAsync(
        Guid userId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}
