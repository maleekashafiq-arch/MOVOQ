using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOVOQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DrawMarketplaceAdminFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "lucky_draws",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "lucky_draws",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "lucky_draws",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FulfillmentStatus",
                table: "lucky_draw_winners",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_lucky_draws_Status_EndAt",
                table: "lucky_draws",
                columns: new[] { "Status", "EndAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lucky_draws_Status_EndAt",
                table: "lucky_draws");

            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "lucky_draws");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "lucky_draws");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "lucky_draws");

            migrationBuilder.DropColumn(
                name: "FulfillmentStatus",
                table: "lucky_draw_winners");
        }
    }
}
