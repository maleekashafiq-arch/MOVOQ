using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MOVOQ.Infrastructure.Persistence;
using MOVOQ.Shared;

namespace MOVOQ.Tests.Integration;

[Collection("Persistence sequential")]
public sealed class IdentityApiTests : IAsyncLifetime
{
    private readonly PersistenceWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public IdentityApiTests(PostgresFixture fixture) => _factory = fixture.Factory;

    public async Task InitializeAsync()
    {
        Skip.If(!_factory.CanRun, "PostgreSQL is not available. Set MOVOQ_TEST_DATABASE or start Docker.");
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MovoqDbContext>();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SkippableFact]
    public async Task Me_without_token_returns_unauthorized()
    {
        using var response = await _client.GetAsync($"{ApiRoutePrefixes.V1}/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [SkippableFact]
    public async Task Session_then_profile_then_device_round_trip()
    {
        using var session = await SendAsync(HttpMethod.Post, $"{ApiRoutePrefixes.V1}/auth/session", uid: "uid-api-1", email: "api.user@movoq.test", verified: true);
        Assert.Equal(HttpStatusCode.OK, session.StatusCode);
        var created = await session.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.NotNull(created);
        Assert.False(created.ProfileComplete);

        using var profile = await SendAsync(
            HttpMethod.Put,
            $"{ApiRoutePrefixes.V1}/me/profile",
            uid: "uid-api-1",
            email: "api.user@movoq.test",
            verified: true,
            body: new
            {
                fullName = "Api User",
                country = "PK",
                state = "Punjab",
                city = "Lahore",
                phoneNumber = (string?)null,
                gender = (string?)null,
                dailyTargetSteps = 6000,
            });
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
        var completed = await profile.Content.ReadFromJsonAsync<CurrentUserResponse>();
        Assert.NotNull(completed);
        Assert.True(completed.ProfileComplete);
        Assert.Equal(6000, completed.DailyTargetSteps);

        using var device = await SendAsync(
            HttpMethod.Post,
            $"{ApiRoutePrefixes.V1}/me/devices",
            uid: "uid-api-1",
            email: "api.user@movoq.test",
            verified: true,
            body: new
            {
                deviceIdentifier = "pixel-test",
                platform = DevicePlatform.Android,
                pushToken = "fcm-1",
                deviceModel = "Pixel",
                osVersion = "14",
                commandId = Guid.NewGuid(),
            });
        Assert.Equal(HttpStatusCode.OK, device.StatusCode);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        string uid,
        string email,
        bool verified,
        object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.TryAddWithoutValidation("Authorization", "Bearer test");
        request.Headers.TryAddWithoutValidation("X-Test-Firebase-Uid", uid);
        request.Headers.TryAddWithoutValidation("X-Test-Email", email);
        request.Headers.TryAddWithoutValidation("X-Test-Email-Verified", verified ? "true" : "false");
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await _client.SendAsync(request);
    }

    private sealed record CurrentUserResponse(
        Guid Id,
        string Email,
        bool EmailVerified,
        bool ProfileComplete,
        int? DailyTargetSteps);
}
