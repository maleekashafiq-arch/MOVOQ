using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class FraudSignalConfiguration : IEntityTypeConfiguration<FraudSignal>
{
    public void Configure(EntityTypeBuilder<FraudSignal> builder)
    {
        builder.ToTable("fraud_signals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ReferenceType).HasMaxLength(64);

        builder.HasIndex(x => new { x.UserId, x.DetectedAt });

        builder.HasOne(x => x.User)
            .WithMany(x => x.FraudSignals)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Append-only fraud facts: no soft-delete filter.
    }
}
