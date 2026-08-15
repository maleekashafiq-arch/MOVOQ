using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class UserWalletConfiguration : IEntityTypeConfiguration<UserWallet>
{
    public void Configure(EntityTypeBuilder<UserWallet> builder)
    {
        builder.ToTable("user_wallets");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
