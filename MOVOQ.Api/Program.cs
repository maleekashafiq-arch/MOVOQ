using MOVOQ.Api.Hosting;
using MOVOQ.Infrastructure;
using MOVOQ.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

await PersistenceStartup.ApplyMigrationsIfEnabledAsync(app.Services, app.Configuration);

app.MapMoVoqEndpoints();

app.Run();

public partial class Program;