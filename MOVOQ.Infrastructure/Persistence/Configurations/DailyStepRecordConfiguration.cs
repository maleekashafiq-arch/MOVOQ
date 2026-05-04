using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class DailyStepRecordConfiguration : IEntityTypeConfiguration<DailyStepRecord>
{
    public void Configure(EntityTypeBuilder<DailyStepRecord> builder)
    {
        builder.ToTable("daily_step_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StepDate).HasConversion(
            v => v.ToDateTime(TimeOnly.MinValue),
            v => DateOnly.FromDateTime(v));

        builder.HasIndex(x => new { x.UserId, x.StepDate }).IsUnique();

        builder.HasIndex(x => x.CommandId);

        builder.HasOne(x => x.UserDevice)
            .WithMany()
            .HasForeignKey(x => x.UserDeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
