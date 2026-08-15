using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Application.Features.Identity;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

public sealed class AdminAccessService(
    IAdminUserRepository admins,
    IAuditTrail audit,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IAdminAccessService
{
    /// <summary>Minted by the Development test handler for <c>Bearer test</c>. Not a Firebase uid.</summary>
    public const string LocalTestUserId = "local-dev-user";
    // Scoped service, so one lookup covers the authorisation filter and every handler in the same request.
    private AdminUser? _resolved;

    public async Task<AdminContext> RequireAsync(
        ExternalIdentity identity,
        AdminRole minimumRole,
        CancellationToken cancellationToken = default)
    {
        var admin = await ResolveAsync(identity, cancellationToken);
        var context = ToContext(admin);

        if (!AdminRoles.Satisfies(context.Role, minimumRole))
        {
            throw new AdminException(
                ErrorCodes.AdminRoleRequired,
                $"This action requires the {minimumRole} role or higher.",
                403);
        }

        return context;
    }

    public async Task<AdminSessionDto> SignInAsync(
        ExternalIdentity identity,
        CancellationToken cancellationToken = default)
    {
        var admin = await ResolveAsync(identity, cancellationToken);
        var previousLogin = admin.LastLoginAt;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        admin.LastLoginAt = now;
        admin.UpdatedAt = now;

        audit.Record(
            ToContext(admin),
            AuditActionType.AdminLogin,
            nameof(AdminUser),
            admin.Id,
            previousLogin?.ToString("O"),
            now.ToString("O"));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AdminSessionDto(admin.Id, admin.Email, admin.DisplayName, admin.Role, previousLogin);
    }

    private async Task<AdminUser> ResolveAsync(ExternalIdentity identity, CancellationToken cancellationToken)
    {
        if (_resolved is not null)
        {
            return _resolved;
        }

        var email = identity.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new AdminException(
                ErrorCodes.EmailRequired,
                "The signed-in account has no email address, so it cannot be matched to an operator.",
                401);
        }

        // The local test identity is minted by the Development handler, not Firebase, so it has no inbox to verify.
        if (!identity.EmailVerified && identity.ProviderUserId != LocalTestUserId)
        {
            throw new AdminException(
                ErrorCodes.EmailNotVerified,
                "Verify this email in Firebase before using the back office.",
                403);
        }

        var admin = await admins.GetByEmailAsync(email, cancellationToken);
        if (admin is null)
        {
            throw new AdminException(
                ErrorCodes.AdminAccessDenied,
                "This account is not registered for the MOVOQ back office.",
                403);
        }

        if (admin.Status != AdminStatus.Active)
        {
            throw new AdminException(
                ErrorCodes.AdminAccessDenied,
                $"This operator account is {admin.Status}.",
                403);
        }

        return _resolved = admin;
    }

    private static AdminContext ToContext(AdminUser admin) =>
        new(admin.Id, admin.Email, admin.DisplayName, admin.Role);
}
