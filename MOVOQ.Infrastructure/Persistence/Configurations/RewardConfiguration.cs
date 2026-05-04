using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.ToTable("rewards");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.TermsAndConditions).HasMaxLength(8000);

        builder.HasOne(x => x.Sponsor)
            .WithMany(x => x.Rewards)
            .HasForeignKey(x => x.SponsorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
