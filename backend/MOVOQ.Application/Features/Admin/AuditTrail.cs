using MOVOQ.Application.Abstractions;
using MOVOQ.Application.Abstractions.Persistence;
using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

public sealed class AuditTrail(
    IAuditLogRepository entries,
    ICallerContext caller,
    TimeProvider timeProvider) : IAuditTrail
{
    public void Record(
        AdminContext admin,
        AuditActionType action,
        string entityName,
        Guid? entityId = null,
        string? oldValue = null,
        string? newValue = null)
    {
        entries.Add(new AuditLog
        {
            AdminUserId = admin.AdminUserId,
            ActionType = action,
            EntityName = entityName,
            EntityId = entityId,
            OldValue = Truncate(oldValue),
            NewValue = Truncate(newValue),
            IpAddress = caller.IpAddress,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
        });
    }

    // The column is capped at 8000 characters; a settings diff should never lose the whole audit row to it.
    private static string? Truncate(string? value) =>
        value is { Length: > 8000 } ? value[..8000] : value;
}
