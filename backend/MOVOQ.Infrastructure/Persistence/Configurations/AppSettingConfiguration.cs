using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("app_settings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Value).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);

        builder.HasIndex(x => x.Key).IsUnique();

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
