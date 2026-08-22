using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MOVOQ.Infrastructure.Persistence;

#nullable disable

namespace MOVOQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(MovoqDbContext))]
    [Migration("20260822133000_WalletBalanceGuard")]
    public partial class WalletBalanceGuard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "ck_user_wallets_balance_non_negative",
                table: "user_wallets",
                sql: "\"Balance\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_user_wallets_balance_non_negative",
                table: "user_wallets");
        }
    }
}
