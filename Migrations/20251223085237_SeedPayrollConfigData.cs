using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedPayrollConfigData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- NHÓM 1: CÁC MỨC LƯƠNG/THU NHẬP CHUẨN ---
            migrationBuilder.InsertData(
                table: "PayrollConfig",
                columns: new[] { "Key", "Value", "Description" },
                values: new object[]
                {
                    "MIN_WAGE_REGION_1_2026",
                    "5310000",
                    "Lương tối thiểu vùng 1 năm 2026. Dùng để tính SÀN đóng BH và TRẦN BHTN."
                });

            migrationBuilder.InsertData(
                table: "PayrollConfig",
                columns: new[] { "Key", "Value", "Description" },
                values: new object[]
                {
                    "GOV_BASE_SALARY",
                    "2340000",
                    "Lương cơ sở (nhà nước). Dùng để tính TRẦN BHXH và BHYT (x20 lần)."
                });

            // --- NHÓM 2: CÁC TỶ LỆ & HỆ SỐ ---
            migrationBuilder.InsertData(
                table: "PayrollConfig",
                columns: new[] { "Key", "Value", "Description" },
                values: new object[]
                {
                    "TRAINED_WORKER_RATE",
                    "1.07",
                    "Tỷ lệ cộng thêm cho lao động qua đào tạo (107% = Lương vùng + 7%)."
                });

            migrationBuilder.InsertData(
                table: "PayrollConfig",
                columns: new[] { "Key", "Value", "Description" },
                values: new object[]
                {
                    "INSURANCE_CAP_RATIO",
                    "20",
                    "Hệ số trần bảo hiểm (Đóng tối đa trên 20 lần mức lương chuẩn)."
                });

            // --- NHÓM 3: THUẾ TNCN (Luật 2026) ---
            migrationBuilder.InsertData(
                table: "PayrollConfig",
                columns: new[] { "Key", "Value", "Description" },
                values: new object[]
                {
                    "PERSONAL_DEDUCTION",
                    "15500000",
                    "Mức giảm trừ gia cảnh cho bản thân (15.5 triệu)."
                });

            migrationBuilder.InsertData(
                table: "PayrollConfig",
                columns: new[] { "Key", "Value", "Description" },
                values: new object[]
                {
                    "DEPENDENT_DEDUCTION",
                    "6200000",
                    "Mức giảm trừ cho mỗi người phụ thuộc (6.2 triệu)."
                });

            migrationBuilder.InsertData(
                table: "PayrollConfig",
                columns: new[] { "Key", "Value", "Description" },
                values: new object[]
                {
                    "FLAT_TAX_THRESHOLD",
                    "2000000",
                    "Ngưỡng thu nhập vãng lai bắt đầu phải khấu trừ 10% (2 triệu)."
                });

            // --- NHÓM 4: CẤU HÌNH MẶC ĐỊNH HỆ THỐNG ---
            migrationBuilder.InsertData(
                table: "PayrollConfig",
                columns: new[] { "Key", "Value", "Description" },
                values: new object[]
                {
                    "DEFAULT_INSURANCE_MODE",
                    "MINIMAL",
                    "Chế độ đóng bảo hiểm mặc định: MINIMAL (Đóng mức sàn) hoặc FULL (Đóng full lương)."
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "MIN_WAGE_REGION_1_2026");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "GOV_BASE_SALARY");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "TRAINED_WORKER_RATE");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "INSURANCE_CAP_RATIO");

            migrationBuilder.DeleteData(
                table: "PayrollConfig",
                keyColumn: "Key",
                keyValue: "PERSONAL_DEDUCTION");

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
                keyValue: "DEFAULT_INSURANCE_MODE");
        }
    }
}
