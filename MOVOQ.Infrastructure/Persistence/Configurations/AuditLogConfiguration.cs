using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.OldValue).HasMaxLength(8000);
        builder.Property(x => x.NewValue).HasMaxLength(8000);
        builder.Property(x => x.IpAddress).HasMaxLength(64);

        // Append-only audit trail: no soft-delete filter.
    }
}
