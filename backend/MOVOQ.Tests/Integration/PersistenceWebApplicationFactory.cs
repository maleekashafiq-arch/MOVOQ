using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace MOVOQ.Tests.Integration;

/// <summary>
/// Host factory backed by PostgreSQL: <c>MOVOQ_TEST_DATABASE</c> if set, otherwise Testcontainers.
/// </summary>
public sealed class PersistenceWebApplicationFactory : WebApplicationFactory<Program>
{
    private PostgreSqlContainer? _container;
    private string? _connectionString;

    public bool CanRun { get; private set; }

    public async Task StartDatabaseAsync()
    {
        var external = Environment.GetEnvironmentVariable("MOVOQ_TEST_DATABASE");
        if (!string.IsNullOrWhiteSpace(external))
        {
            _connectionString = external;
            CanRun = true;
            return;
        }

        try
        {
            _container = new PostgreSqlBuilder("postgres:17-alpine").Build();
            await _container.StartAsync();
            _connectionString = _container.GetConnectionString();
            CanRun = true;
        }
        catch
        {
            CanRun = false;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Persistence:ApplyMigrationsAtStartup", "false");
        builder.UseSetting(
            "ConnectionStrings:Database",
            _connectionString ?? "Host=127.0.0.1;Port=1;Database=movoq_unavailable;Username=movoq;Password=movoq");
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}

public sealed class PostgresFixture : IAsyncLifetime
{
    public PersistenceWebApplicationFactory Factory { get; } = new();

    public bool CanRun => Factory.CanRun;

    public Task InitializeAsync() => Factory.StartDatabaseAsync();

    public async Task DisposeAsync() => await Factory.DisposeAsync();
}
