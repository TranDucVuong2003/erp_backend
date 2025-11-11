using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryServiceAddonIdToQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryServiceAddonId",
                table: "Quotes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotes_CategoryServiceAddonId",
                table: "Quotes",
                column: "CategoryServiceAddonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_CategoryServiceAddons_CategoryServiceAddonId",
                table: "Quotes",
                column: "CategoryServiceAddonId",
                principalTable: "CategoryServiceAddons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_CategoryServiceAddons_CategoryServiceAddonId",
                table: "Quotes");

            migrationBuilder.DropIndex(
                name: "IX_Quotes_CategoryServiceAddonId",
                table: "Quotes");

            migrationBuilder.DropColumn(
                name: "CategoryServiceAddonId",
                table: "Quotes");
        }
    }
}
