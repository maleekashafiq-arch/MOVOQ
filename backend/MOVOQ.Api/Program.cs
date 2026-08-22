using System.Text.Json.Serialization;
using MOVOQ.Api.Hosting;
using MOVOQ.Application.Abstractions;
using MOVOQ.Infrastructure;
using MOVOQ.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Enums travel as names so clients read "Validated" rather than a number whose meaning shifts with the enum.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

// The admin app runs on its own origin. Development stays wide open; anywhere else the origins must be
// listed under Cors:AllowedOrigins or the browser will refuse the call.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod();

        if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(allowedOrigins).AllowCredentials();
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICallerContext, HttpCallerContext>();
builder.Services.AddMoVoqAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddMoVoqRateLimiting(builder.Environment);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment() || allowedOrigins.Length > 0)
{
    app.UseCors();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseMoVoqRateLimiting();

await PersistenceStartup.ApplyMigrationsIfEnabledAsync(app.Services, app.Configuration);
await DevEndpoints.SeedIfDevelopmentAsync(app);
await AdminBootstrap.EnsureOperatorsAsync(app);

app.MapMoVoqEndpoints();

app.Run();

public partial class Program;
