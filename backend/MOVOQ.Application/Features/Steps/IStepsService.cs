using MOVOQ.Application.Features.Identity;

namespace MOVOQ.Application.Features.Steps;

public interface IStepsService
{
    /// <summary>Applies a device claim for one day, awarding goal points at most once per day.</summary>
    Task<StepsSyncResultDto> SyncAsync(
        ExternalIdentity identity,
        SyncStepsCommand command,
        CancellationToken cancellationToken = default);

    Task<StepsSyncResultDto> GetTodayAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default);

    Task<StepsHistoryDto> GetHistoryAsync(
        ExternalIdentity identity,
        int days,
        CancellationToken cancellationToken = default);
}
