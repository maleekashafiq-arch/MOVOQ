using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class RewardRedemptionConfiguration : IEntityTypeConfiguration<RewardRedemption>
{
    public void Configure(EntityTypeBuilder<RewardRedemption> builder)
    {
        builder.ToTable("reward_redemptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AssignedCode).HasMaxLength(256);
        builder.Property(x => x.FailureReason).HasMaxLength(1024);

        builder.HasOne(x => x.Reward)
            .WithMany(x => x.Redemptions)
            .HasForeignKey(x => x.RewardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(x => x.RewardRedemptions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PointsLedger)
            .WithMany()
            .HasForeignKey(x => x.PointsLedgerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
