using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyTaxModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Taxes_IsActive",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_TaxCode",
                table: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_Taxes_TaxType",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "ApplicableFor",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "TaxCode",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "TaxType",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "Tax_Amount",
                table: "Taxes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicableFor",
                table: "Taxes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Taxes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFrom",
                table: "Taxes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveTo",
                table: "Taxes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Taxes",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Taxes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxCode",
                table: "Taxes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxType",
                table: "Taxes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "VAT");

            migrationBuilder.AddColumn<decimal>(
                name: "Tax_Amount",
                table: "Taxes",
                type: "numeric(15,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_IsActive",
                table: "Taxes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_TaxCode",
                table: "Taxes",
                column: "TaxCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_TaxType",
                table: "Taxes",
                column: "TaxType");
        }
    }
}
