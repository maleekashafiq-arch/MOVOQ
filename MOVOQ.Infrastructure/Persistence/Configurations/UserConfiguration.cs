using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(320);
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.PasswordHashReference).HasMaxLength(512);

        builder.HasIndex(x => x.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
        builder.HasIndex(x => x.PhoneNumber).IsUnique().HasFilter("[PhoneNumber] IS NOT NULL");

        builder.HasQueryFilter(x => x.DeletedAt == null);

        builder.HasOne(x => x.Profile)
            .WithOne(x => x.User)
            .HasForeignKey<UserProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Wallet)
            .WithOne(x => x.User)
            .HasForeignKey<UserWallet>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Devices)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.DailyStepRecords)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.StepGoals)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.PointsLedgers)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.AdsWatched)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
