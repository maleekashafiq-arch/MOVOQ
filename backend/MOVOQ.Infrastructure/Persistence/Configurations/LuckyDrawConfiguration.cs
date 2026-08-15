using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class LuckyDrawConfiguration : IEntityTypeConfiguration<LuckyDraw>
{
    public void Configure(EntityTypeBuilder<LuckyDraw> builder)
    {
        builder.ToTable("lucky_draws");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.PrizeDescription).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Emoji).HasMaxLength(16);
        builder.Property(x => x.ImageUrl).HasMaxLength(2048);

        builder.HasIndex(x => new { x.Status, x.EndAt });

        builder.HasMany(x => x.Tickets)
            .WithOne(x => x.LuckyDraw)
            .HasForeignKey(x => x.LuckyDrawId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
