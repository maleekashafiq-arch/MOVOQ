using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MOVOQ.Domain.Entities;

namespace MOVOQ.Infrastructure.Persistence.Configurations;

internal sealed class UserWalletConfiguration : IEntityTypeConfiguration<UserWallet>
{
    public void Configure(EntityTypeBuilder<UserWallet> builder)
    {
        builder.ToTable("user_wallets", t =>
            t.HasCheckConstraint("ck_user_wallets_balance_non_negative", "\"Balance\" >= 0"));

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
