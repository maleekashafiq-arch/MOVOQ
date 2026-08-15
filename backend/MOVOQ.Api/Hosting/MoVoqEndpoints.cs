using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MOVOQ.Shared;

namespace MOVOQ.Api.Hosting;

internal static class MoVoqEndpoints
{
    internal static WebApplication MapMoVoqEndpoints(this WebApplication app)
    {
        MapSystemEndpoints(app);
        MapApiV1(app);
        app.MapDevDashboard();
        return app;
    }

    private static void MapSystemEndpoints(WebApplication app)
    {
        app.MapHealthChecks(
                ApiRoutePrefixes.Health,
                new HealthCheckOptions
                {
                    AllowCachingResponses = false,
                    ResponseWriter = WriteHealthJsonAsync,
                })
            .WithName("Health")
            .WithTags("system")
            .WithSummary("Liveness/readiness style health probe (unversioned).");
    }

    private static void MapApiV1(WebApplication app)
    {
        var v1 = app.MapGroup(ApiRoutePrefixes.V1)
            .WithTags("v1");

        v1.MapGet(string.Empty, () => Results.Ok(new ApiV1MetaResponse("MOVOQ", "v1")))
            .WithName("GetApiV1Meta")
            .WithSummary("API v1 metadata.");

        v1.MapIdentityEndpoints();
        v1.MapStepsEndpoints();
        v1.MapLuckyDrawEndpoints();
        v1.MapAdminEndpoints();
    }

    private static Task WriteHealthJsonAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new HealthReportDto(
            report.Status.ToString(),
            report.TotalDuration,
            report.Entries.ToDictionary(
                static e => e.Key,
                static e => new HealthCheckEntryDto(
                    e.Value.Status.ToString(),
                    e.Value.Description,
                    e.Value.Duration)));

        return context.Response.WriteAsJsonAsync(payload, context.RequestAborted);
    }

    private sealed record HealthReportDto(string Status, TimeSpan TotalDuration, IReadOnlyDictionary<string, HealthCheckEntryDto> Checks);

    private sealed record HealthCheckEntryDto(string Status, string? Description, TimeSpan Duration);

    private sealed record ApiV1MetaResponse(string Service, string ApiVersion);
}
