using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDurationAndTemplateToSaleOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "duration",
                table: "SaleOrderServices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "template",
                table: "SaleOrderServices",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "duration",
                table: "SaleOrderAddons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "template",
                table: "SaleOrderAddons",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "duration",
                table: "SaleOrderServices");

            migrationBuilder.DropColumn(
                name: "template",
                table: "SaleOrderServices");

            migrationBuilder.DropColumn(
                name: "duration",
                table: "SaleOrderAddons");

            migrationBuilder.DropColumn(
                name: "template",
                table: "SaleOrderAddons");
        }
    }
}
