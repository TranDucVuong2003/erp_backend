using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedPayrollConfigDataInModelBuilder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PayrollConfig",
                columns: new[] { "Key", "Description", "Value" },
                values: new object[,]
                {
                    { "DEFAULT_INSURANCE_MODE", "Chế độ đóng bảo hiểm mặc định: MINIMAL (Đóng mức sàn) hoặc FULL (Đóng full lương).", "MINIMAL" },
                    { "DEPENDENT_DEDUCTION", "Mức giảm trừ cho mỗi người phụ thuộc (6.2 triệu).", "6200000" },
                    { "FLAT_TAX_THRESHOLD", "Ngưỡng thu nhập vãng lai bắt đầu phải khấu trừ 10% (2 triệu).", "2000000" },
                    { "GOV_BASE_SALARY", "Lương cơ sở (nhà nước). Dùng để tính TRẦN BHXH và BHYT (x20 lần).", "2340000" },
                    { "INSURANCE_CAP_RATIO", "Hệ số trần bảo hiểm (Đóng tối đa trên 20 lần mức lương chuẩn).", "20" },
                    { "MIN_WAGE_REGION_1_2026", "Lương tối thiểu vùng 1 năm 2026. Dùng để tính SÀN đóng BH và TRẦN BHTN.", "5310000" },
                    { "PERSONAL_DEDUCTION", "Mức giảm trừ gia cảnh cho bản thân (15.5 triệu).", "15500000" },
                    { "TRAINED_WORKER_RATE", "Tỷ lệ cộng thêm cho lao động qua đào tạo (107% = Lương vùng + 7%).", "1.07" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "DEFAULT_INSURANCE_MODE");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "DEPENDENT_DEDUCTION");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "FLAT_TAX_THRESHOLD");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "GOV_BASE_SALARY");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "INSURANCE_CAP_RATIO");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "MIN_WAGE_REGION_1_2026");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "PERSONAL_DEDUCTION");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "TRAINED_WORKER_RATE");
        }
    }
}
