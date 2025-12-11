using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryAndTypeColumnsActual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Services_Category",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Addons_Type",
                table: "Addons");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Addons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Services",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Addons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_Category",
                table: "Services",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Addons_Type",
                table: "Addons",
                column: "Type");
        }
    }
}
