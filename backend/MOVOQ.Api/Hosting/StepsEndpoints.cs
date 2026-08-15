using MOVOQ.Application.Features.Steps;

namespace MOVOQ.Api.Hosting;

internal static class StepsEndpoints
{
    internal static IEndpointRouteBuilder MapStepsEndpoints(this IEndpointRouteBuilder v1)
    {
        var steps = v1.MapGroup("/steps").WithTags("steps").RequireAuthorization();

        steps.MapPost("/sync", SyncSteps)
            .WithName("SyncSteps")
            .WithSummary("Submit a device's step claim for one day and receive the server verdict.");

        steps.MapGet("/today", GetToday)
            .WithName("GetTodaySteps")
            .WithSummary("Return today's validated steps, wallet balance, and streak.");

        steps.MapGet("/history", GetHistory)
            .WithName("GetStepsHistory")
            .WithSummary("Return one entry per day for the requested window, gaps filled with zeroes.");

        return v1;
    }

    private static Task<IResult> SyncSteps(
        HttpContext http,
        SyncStepsRequest body,
        IStepsService steps,
        CancellationToken cancellationToken)
    {
        var command = new SyncStepsCommand(
            body.CommandId,
            body.StepDate,
            body.StepCount,
            body.DeviceIdentifier,
            body.DeviceRecordedAt);

        return ApiResults.ExecuteAsync(() =>
            steps.SyncAsync(http.User.ReadExternalIdentity(), command, cancellationToken));
    }

    private static Task<IResult> GetToday(HttpContext http, IStepsService steps, CancellationToken cancellationToken) =>
        ApiResults.ExecuteAsync(() => steps.GetTodayAsync(http.User.ReadExternalIdentity(), cancellationToken));

    private static Task<IResult> GetHistory(
        HttpContext http,
        IStepsService steps,
        CancellationToken cancellationToken,
        int days = 7) =>
        ApiResults.ExecuteAsync(() => steps.GetHistoryAsync(http.User.ReadExternalIdentity(), days, cancellationToken));

    private sealed record SyncStepsRequest(
        Guid CommandId,
        DateOnly StepDate,
        int StepCount,
        string? DeviceIdentifier,
        DateTime? DeviceRecordedAt);
}
