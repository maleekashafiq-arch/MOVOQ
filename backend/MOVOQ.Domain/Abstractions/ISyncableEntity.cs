using MOVOQ.Domain.Entities;
using MOVOQ.Shared;

namespace MOVOQ.Domain.Abstractions;

/// <summary>
/// Optional contract for entities that participate in offline sync and idempotent command processing.
/// </summary>
/// <remarks>
/// Any entity implementing <see cref="ISyncableEntity"/> must also inherit <see cref="BaseEntity"/>.
/// </remarks>
public interface ISyncableEntity
{
    /// <summary>Client-generated idempotency key for offline and retry-safe commands.</summary>
    Guid CommandId { get; set; }

    /// <summary>Originating device when applicable; null for server- or admin-generated records.</summary>
    Guid? DeviceId { get; set; }

    /// <summary>Origin of the change (mobile, server, or admin).</summary>
    DataSource Source { get; set; }

    /// <summary>UTC instant when the server last acknowledged sync for this record, if any.</summary>
    DateTime? SyncedAt { get; set; }
}
