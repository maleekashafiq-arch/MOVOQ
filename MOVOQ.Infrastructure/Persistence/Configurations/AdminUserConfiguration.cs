using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("admin_users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(256);
        builder.Property(x => x.PasswordHashReference).HasMaxLength(512);

        builder.HasIndex(x => x.Email).IsUnique();

        builder.HasMany(x => x.AuditLogs)
            .WithOne(x => x.AdminUser)
            .HasForeignKey(x => x.AdminUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
