using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfStorageFieldsToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractPdfPath",
                table: "Contracts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PdfFileSize",
                table: "Contracts",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PdfGeneratedAt",
                table: "Contracts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractPdfPath",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "PdfFileSize",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "PdfGeneratedAt",
                table: "Contracts");
        }
    }
}
