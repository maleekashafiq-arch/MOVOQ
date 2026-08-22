using MOVOQ.Application.Features.Identity;
using MOVOQ.Shared;

namespace MOVOQ.Api.Hosting;

internal static class IdentityEndpoints
{
    internal static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder v1)
    {
        var auth = v1.MapGroup("/auth").WithTags("identity");
        auth.MapPost("/session", ExchangeSession)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .WithName("ExchangeSession")
            .WithSummary("Create or resume a MOVOQ user from the Firebase (or test) identity token.");

        var me = v1.MapGroup("/me").WithTags("identity").RequireAuthorization();
        me.MapGet(string.Empty, GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Return the signed-in user, provisioning the account on first call.");
        me.MapPut("/profile", CompleteProfile)
            .WithName("CompleteProfile")
            .WithSummary("Save mandatory profile fields and daily step goal.");
        me.MapPost("/devices", RegisterDevice)
            .WithName("RegisterDevice")
            .WithSummary("Register or refresh the current mobile device.");

        return v1;
    }

    private static async Task<IResult> ExchangeSession(HttpContext http, IIdentityService identity, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(() => identity.ExchangeSessionAsync(http.User.ReadExternalIdentity(), cancellationToken));
    }

    private static async Task<IResult> GetCurrentUser(HttpContext http, IIdentityService identity, CancellationToken cancellationToken)
    {
        return await ExecuteAsync(() => identity.GetCurrentUserAsync(http.User.ReadExternalIdentity(), cancellationToken));
    }

    private static async Task<IResult> CompleteProfile(
        HttpContext http,
        CompleteProfileRequest body,
        IIdentityService identity,
        CancellationToken cancellationToken)
    {
        var command = new CompleteProfileCommand(
            body.FullName,
            body.Country,
            body.State,
            body.City,
            body.PhoneNumber,
            body.Gender,
            body.DailyTargetSteps);

        return await ExecuteAsync(() => identity.CompleteProfileAsync(http.User.ReadExternalIdentity(), command, cancellationToken));
    }

    private static async Task<IResult> RegisterDevice(
        HttpContext http,
        RegisterDeviceRequest body,
        IIdentityService identity,
        CancellationToken cancellationToken)
    {
        var command = new RegisterDeviceCommand(
            body.DeviceIdentifier,
            body.Platform,
            body.PushToken,
            body.DeviceModel,
            body.OsVersion,
            body.CommandId);

        return await ExecuteAsync(() => identity.RegisterDeviceAsync(http.User.ReadExternalIdentity(), command, cancellationToken));
    }

    private static Task<IResult> ExecuteAsync<T>(Func<Task<T>> action) => ApiResults.ExecuteAsync(action);

    private sealed record CompleteProfileRequest(
        string FullName,
        string Country,
        string State,
        string City,
        string? PhoneNumber,
        string? Gender,
        int DailyTargetSteps);

    private sealed record RegisterDeviceRequest(
        string DeviceIdentifier,
        DevicePlatform Platform,
        string? PushToken,
        string? DeviceModel,
        string? OsVersion,
        Guid CommandId);
}
