using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQuantityNotesUnitPriceFromQuoteServiceAndQuoteAddon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "QuoteServices");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "QuoteServices");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "QuoteServices");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "QuoteAddons");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "QuoteAddons");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "QuoteAddons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "QuoteServices",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "QuoteServices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "QuoteServices",
                type: "numeric(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "QuoteAddons",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "QuoteAddons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "QuoteAddons",
                type: "numeric(15,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
