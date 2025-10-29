using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class MakeServiceIdRequiredInSaleOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Xóa các SaleOrder có ServiceId = NULL (nếu không hợp lệ)
            // HOẶC gán cho chúng một ServiceId hợp lệ
            migrationBuilder.Sql(@"
                DELETE FROM ""SaleOrders"" 
                WHERE ""ServiceId"" IS NULL;
            ");

            // Bước 2: Thay đổi cột ServiceId từ nullable thành non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "SaleOrders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "SaleOrders",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
