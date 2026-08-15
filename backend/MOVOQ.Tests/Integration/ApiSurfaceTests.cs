using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MOVOQ.Shared;

namespace MOVOQ.Tests.Integration;

public sealed class ApiSurfaceTests(ApiWebApplicationFactory factory) : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });

    [Fact]
    public async Task Health_returns_json_and_ok()
    {
        using var response = await _client.GetAsync(ApiRoutePrefixes.Health);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadFromJsonAsync<HealthReportDto>();
        Assert.NotNull(body);
        Assert.Equal("Healthy", body.Status);
        Assert.NotNull(body.Checks);
    }

    [Fact]
    public async Task Me_without_token_returns_unauthorized()
    {
        using var response = await _client.GetAsync($"{ApiRoutePrefixes.V1}/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Api_v1_root_returns_meta()
    {
        using var response = await _client.GetAsync($"{ApiRoutePrefixes.V1}/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiV1MetaResponse>();
        Assert.NotNull(body);
        Assert.Equal("MOVOQ", body.Service);
        Assert.Equal("v1", body.ApiVersion);
    }

    private sealed record HealthReportDto(string Status, TimeSpan TotalDuration, IReadOnlyDictionary<string, HealthCheckEntryDto> Checks);

    private sealed record HealthCheckEntryDto(string Status, string? Description, TimeSpan Duration);

    private sealed record ApiV1MetaResponse(string Service, string ApiVersion);
}
