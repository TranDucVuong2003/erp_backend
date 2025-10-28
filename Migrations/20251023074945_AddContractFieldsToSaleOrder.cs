using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddContractFieldsToSaleOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleOrders_Probability",
                table: "SaleOrders");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "SaleOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractDate",
                table: "SaleOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractNumber",
                table: "SaleOrders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SaleOrders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_ContractNumber",
                table: "SaleOrders",
                column: "ContractNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_Status",
                table: "SaleOrders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleOrders_ContractNumber",
                table: "SaleOrders");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrders_Status",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "ContractDate",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "ContractNumber",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SaleOrders");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_Probability",
                table: "SaleOrders",
                column: "Probability");
        }
    }
}
