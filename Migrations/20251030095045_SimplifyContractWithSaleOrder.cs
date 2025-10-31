using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyContractWithSaleOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ XÓA TẤT CẢ DỮ LIỆU CŨ TRƯỚC KHI THAY ĐỔI SCHEMA
            migrationBuilder.Sql("DELETE FROM \"Contracts\";");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Addons_AddonsId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Customers_CustomerId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Services_ServiceId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Taxes_TaxId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_AddonsId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ServiceId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_TaxId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "AddonsId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Contracts",
                newName: "SaleOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_CustomerId",
                table: "Contracts",
                newName: "IX_Contracts_SaleOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_SaleOrders_SaleOrderId",
                table: "Contracts",
                column: "SaleOrderId",
                principalTable: "SaleOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_SaleOrders_SaleOrderId",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "SaleOrderId",
                table: "Contracts",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Contracts_SaleOrderId",
                table: "Contracts",
                newName: "IX_Contracts_CustomerId");

            migrationBuilder.AddColumn<int>(
                name: "AddonsId",
                table: "Contracts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Contracts",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "Contracts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaxId",
                table: "Contracts",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_AddonsId",
                table: "Contracts",
                column: "AddonsId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ServiceId",
                table: "Contracts",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_TaxId",
                table: "Contracts",
                column: "TaxId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Addons_AddonsId",
                table: "Contracts",
                column: "AddonsId",
                principalTable: "Addons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Customers_CustomerId",
                table: "Contracts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Services_ServiceId",
                table: "Contracts",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Taxes_TaxId",
                table: "Contracts",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
