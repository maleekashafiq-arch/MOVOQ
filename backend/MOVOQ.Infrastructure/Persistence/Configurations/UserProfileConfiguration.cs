using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Property(x => x.DisplayName).HasMaxLength(256);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048);
        builder.Property(x => x.Gender).HasMaxLength(32);
        builder.Property(x => x.Country).HasMaxLength(2);
        builder.Property(x => x.State).HasMaxLength(128);
        builder.Property(x => x.City).HasMaxLength(128);
        builder.Property(x => x.PreferredLanguage).HasMaxLength(32);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
