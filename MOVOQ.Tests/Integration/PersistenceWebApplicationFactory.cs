using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MOVOQ.Infrastructure.Persistence;

namespace MOVOQ.Tests.Integration;

/// <summary>
/// Host factory that pins EF to a shared in-memory SQLite connection so migrations and tests see one database.
/// </summary>
public sealed class PersistenceWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public PersistenceWebApplicationFactory()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = $"movoq-persistence-tests-{Guid.NewGuid():n}",
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        };

        _connection = new SqliteConnection(builder.ConnectionString);
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Persistence:ApplyMigrationsAtStartup", "false");

        builder.ConfigureTestServices(services =>
        {
            var descriptors = services.Where(static d =>
                    d.ServiceType == typeof(MovoqDbContext)
                    || (d.ServiceType.IsGenericType
                        && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)
                        && d.ServiceType.GenericTypeArguments.Length == 1
                        && d.ServiceType.GenericTypeArguments[0] == typeof(MovoqDbContext)))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<MovoqDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
