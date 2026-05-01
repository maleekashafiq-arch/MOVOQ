using MOVOQ.Domain.Enums;

namespace MOVOQ.Domain.Abstractions;

public interface ISyncableEntity
{
    Guid? DeviceId { get; set; }
    DateTime? SyncedAt { get; set; }
    DataSource Source { get; set; }
}
