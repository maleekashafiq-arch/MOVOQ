using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class PointsLedgerConfiguration : IEntityTypeConfiguration<PointsLedger>
{
    public void Configure(EntityTypeBuilder<PointsLedger> builder)
    {
        builder.ToTable("points_ledgers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1024);

        builder.HasIndex(x => new { x.UserId, x.OccurredAt });

        // Append-only ledger: do not apply soft-delete filters or mutators here (enforced in Application later).
    }
}
