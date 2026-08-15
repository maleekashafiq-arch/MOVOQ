using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class ProcessedCommandConfiguration : IEntityTypeConfiguration<ProcessedCommand>
{
    public void Configure(EntityTypeBuilder<ProcessedCommand> builder)
    {
        builder.ToTable("processed_commands");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CommandId).IsUnique();

        builder.HasIndex(x => x.ProcessedAt);

        builder.Property(x => x.CommandType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RelatedEntityName).HasMaxLength(256);

        builder.HasOne(x => x.User)
            .WithMany(x => x.ProcessedCommands)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Idempotency receipts are insert-only; no soft-delete filter.
    }
}
