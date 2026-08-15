using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class UserAuthProviderConfiguration : IEntityTypeConfiguration<UserAuthProvider>
{
    public void Configure(EntityTypeBuilder<UserAuthProvider> builder)
    {
        builder.ToTable("user_auth_providers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderUserId).HasMaxLength(256).IsRequired();
        builder.Property(x => x.EmailAtLink).HasMaxLength(320);

        builder.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
        builder.HasIndex(x => x.UserId);

        builder.HasQueryFilter(x => x.DeletedAt == null);

        builder.HasOne(x => x.User)
            .WithMany(x => x.AuthProviders)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
