using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.ToTable("user_devices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DeviceIdentifier).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PushToken).HasMaxLength(512);
        builder.Property(x => x.DeviceModel).HasMaxLength(128);
        builder.Property(x => x.OsVersion).HasMaxLength(64);

        builder.HasIndex(x => new { x.UserId, x.DeviceIdentifier }).IsUnique();

        builder.HasIndex(x => x.CommandId);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
