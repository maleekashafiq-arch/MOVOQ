using MOVOQ.Shared;

namespace MOVOQ.Application.Features.Admin;

/// <summary>
/// Records privileged back-office actions. Entries are staged with the caller's unit of work so an action
/// and its audit row commit together — an action that rolls back must not leave an audit trail behind.
/// </summary>
public interface IAuditTrail
{
    void Record(
        AdminContext admin,
        AuditActionType action,
        string entityName,
        Guid? entityId = null,
        string? oldValue = null,
        string? newValue = null);
}
