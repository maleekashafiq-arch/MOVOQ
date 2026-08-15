using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

public sealed class AdminOperatorsService(
    IAdminUserRepository operators,
    IAuditTrail audit,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IAdminOperatorsService
{
    public async Task<AdminOperatorsReportDto> ListAsync(CancellationToken cancellationToken = default)
    {
        var items = await operators.ListAsync(cancellationToken);
        var today = timeProvider.GetUtcNow().UtcDateTime.Date;

        return new AdminOperatorsReportDto(
            items.Count,
            items.Count(x => x.Role == AdminRole.SuperAdmin && x.Status == AdminStatus.Active),
            items.Count(x => x.LastLoginAt >= today),
            items.Select(Map).ToList());
    }

    public async Task<AdminOperatorDto> CreateAsync(
        AdminContext admin,
        SaveOperatorCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(command.Email);
        Validate(command, email);

        if (await operators.GetByEmailAsync(email, cancellationToken) is not null)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, $"{email} is already an operator.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var created = new AdminUser
        {
            Email = email,
            DisplayName = Clean(command.DisplayName) ?? email[..email.IndexOf('@')],
            Role = command.Role,
            Status = command.Status,
            CreatedAt = now,
        };

        operators.Add(created);
        audit.Record(
            admin,
            AuditActionType.AdminUserModified,
            nameof(AdminUser),
            created.Id,
            null,
            $"{created.Email}, {created.Role}, {created.Status}");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(created);
    }

    public async Task<AdminOperatorDto> UpdateAsync(
        AdminContext admin,
        Guid operatorId,
        SaveOperatorCommand command,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(command.Email);
        Validate(command, email);

        var target = await operators.GetByIdAsync(operatorId, cancellationToken)
            ?? throw new AdminException(ErrorCodes.NotFound, $"No operator with id {operatorId}.", 404);

        var clash = await operators.GetByEmailAsync(email, cancellationToken);
        if (clash is not null && clash.Id != target.Id)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, $"{email} is already an operator.");
        }

        if (WouldLeaveNoSuperAdmin(await operators.ListAsync(cancellationToken), target.Id, command.Role, command.Status))
        {
            throw new AdminException(
                ErrorCodes.InvalidStateTransition,
                "The back office must keep at least one active SuperAdmin.",
                409);
        }

        var before = $"{target.Email}, {target.Role}, {target.Status}";
        target.Email = email;
        target.DisplayName = Clean(command.DisplayName) ?? target.DisplayName;
        target.Role = command.Role;
        target.Status = command.Status;
        target.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;

        audit.Record(
            admin,
            AuditActionType.AdminUserModified,
            nameof(AdminUser),
            target.Id,
            before,
            $"{target.Email}, {target.Role}, {target.Status}");

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(target);
    }

    private static bool WouldLeaveNoSuperAdmin(
        IReadOnlyList<AdminUser> all,
        Guid targetId,
        AdminRole newRole,
        AdminStatus newStatus)
    {
        var others = all.Any(x =>
            x.Id != targetId && x.Role == AdminRole.SuperAdmin && x.Status == AdminStatus.Active);
        var selfStill = newRole == AdminRole.SuperAdmin && newStatus == AdminStatus.Active;
        return !others && !selfStill;
    }

    private static void Validate(SaveOperatorCommand command, string email)
    {
        if (email.Length == 0 || email.IndexOf('@') <= 0)
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "An operator needs a real email address.");
        }

        if (!Enum.IsDefined(command.Role))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Unknown role.");
        }

        if (!Enum.IsDefined(command.Status))
        {
            throw new AdminException(ErrorCodes.ValidationFailed, "Unknown operator status.");
        }
    }

    private static string NormalizeEmail(string? email) => email?.Trim().ToLowerInvariant() ?? string.Empty;

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AdminOperatorDto Map(AdminUser admin) => new(
        admin.Id,
        admin.Email,
        admin.DisplayName,
        admin.Role,
        admin.Status,
        admin.LastLoginAt,
        admin.CreatedAt);
}
