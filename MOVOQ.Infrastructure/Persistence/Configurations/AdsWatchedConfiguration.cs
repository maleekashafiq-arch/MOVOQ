using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class AdsWatchedConfiguration : IEntityTypeConfiguration<AdsWatched>
{
    public void Configure(EntityTypeBuilder<AdsWatched> builder)
    {
        builder.ToTable("ads_watched");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AdProvider).HasMaxLength(128);
        builder.Property(x => x.ProviderTransactionId).HasMaxLength(256);

        builder.HasIndex(x => x.AdSessionId).IsUnique();

        builder.HasIndex(x => x.CommandId);

        builder.HasOne(x => x.UserDevice)
            .WithMany()
            .HasForeignKey(x => x.UserDeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
