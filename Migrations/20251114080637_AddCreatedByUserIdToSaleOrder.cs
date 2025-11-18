using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserIdToSaleOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "SaleOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_CreatedByUserId",
                table: "SaleOrders",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_Users_CreatedByUserId",
                table: "SaleOrders",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_Users_CreatedByUserId",
                table: "SaleOrders");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrders_CreatedByUserId",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SaleOrders");
        }
    }
}
