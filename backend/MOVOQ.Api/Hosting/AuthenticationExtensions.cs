using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MOVOQ.Application.Features.Identity;

namespace MOVOQ.Api.Hosting;

internal static class AuthenticationExtensions
{
    internal const string TestScheme = "Test";

    internal static IServiceCollection AddMoVoqAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsEnvironment("Testing"))
        {
            services.AddAuthentication(TestScheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestScheme, _ => { });
            services.AddAuthorization();
            return services;
        }

        var projectId = configuration["Firebase:ProjectId"];
        if (string.IsNullOrWhiteSpace(projectId))
        {
            if (environment.IsDevelopment())
            {
                services.AddAuthentication(TestScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestScheme, _ => { });
                services.AddAuthorization();
                return services;
            }

            throw new InvalidOperationException("Firebase:ProjectId is required outside Testing/Development.");
        }

        var authority = $"https://securetoken.google.com/{projectId}";
        var authentication = services.AddAuthentication(options =>
        {
            if (environment.IsDevelopment())
            {
                options.DefaultAuthenticateScheme = "LocalSmart";
                options.DefaultChallengeScheme = "LocalSmart";
            }
            else
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
        });

        if (environment.IsDevelopment())
        {
            authentication.AddPolicyScheme("LocalSmart", "Firebase or local test", options =>
            {
                options.ForwardDefaultSelector = context =>
                    IsLocalTestRequest(context.Request) ? TestScheme : JwtBearerDefaults.AuthenticationScheme;
            });
            authentication.AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestScheme, _ => { });
        }

        authentication.AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authority,
                ValidateAudience = true,
                ValidAudience = projectId,
                ValidateLifetime = true,
            };
        });
        services.AddAuthorization();
        return services;
    }

    private static bool IsLocalTestRequest(HttpRequest request)
    {
        if (request.Headers.ContainsKey("X-Test-Firebase-Uid") || request.Headers.ContainsKey("X-Test-Email"))
        {
            return true;
        }

        var authorization = request.Headers.Authorization.ToString();
        return authorization.Equals("Bearer test", StringComparison.OrdinalIgnoreCase)
            || authorization.StartsWith("Bearer test ", StringComparison.OrdinalIgnoreCase);
    }

    internal static ExternalIdentity ReadExternalIdentity(this ClaimsPrincipal principal)
    {
        var uid = principal.FindFirstValue("user_id")
            ?? principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? string.Empty;

        var email = principal.FindFirstValue("email")
            ?? principal.FindFirstValue(ClaimTypes.Email)
            ?? string.Empty;

        var verifiedRaw = principal.FindFirstValue("email_verified");
        var emailVerified = verifiedRaw is "true" or "True" or "1";

        var name = principal.FindFirstValue("name")
            ?? principal.FindFirstValue(ClaimTypes.Name);

        return new ExternalIdentity(uid, email, emailVerified, name);
    }
}

internal sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IHostEnvironment environment) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        var isTestBearer = authorization.Equals("Bearer test", StringComparison.OrdinalIgnoreCase)
            || authorization.StartsWith("Bearer test ", StringComparison.OrdinalIgnoreCase);
        var hasTestHeaders = Request.Headers.ContainsKey("X-Test-Firebase-Uid")
            || Request.Headers.ContainsKey("X-Test-Email");

        // Any other Authorization value must go through Firebase JWT validation, even in Development.
        if (!isTestBearer && !hasTestHeaders)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var uid = Request.Headers["X-Test-Firebase-Uid"].ToString();
        var email = Request.Headers["X-Test-Email"].ToString();
        var verified = Request.Headers["X-Test-Email-Verified"].ToString();
        var name = Request.Headers["X-Test-Name"].ToString();

        if (environment.IsDevelopment())
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                uid = "local-dev-user";
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                email = "dev@localhost";
            }

            if (string.IsNullOrWhiteSpace(verified))
            {
                verified = "true";
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Local Walker";
            }
        }

        if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(email))
        {
            return Task.FromResult(AuthenticateResult.Fail("Test identity headers X-Test-Firebase-Uid and X-Test-Email are required."));
        }

        var claims = new List<Claim>
        {
            new("user_id", uid),
            new("sub", uid),
            new("email", email),
            new("email_verified", string.IsNullOrWhiteSpace(verified) ? "false" : verified),
        };

        if (!string.IsNullOrWhiteSpace(name))
        {
            claims.Add(new Claim("name", name));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
