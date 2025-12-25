using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameInsuranceStatusToPayrollConfigAndReseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_InsuranceStatus",
                table: "InsuranceStatus");

            migrationBuilder.RenameTable(
                name: "InsuranceStatus",
                newName: "PayrollConfig");

            migrationBuilder.RenameIndex(
                name: "IX_InsuranceStatus_Key",
                table: "PayrollConfig",
                newName: "IX_PayrollConfig_Key");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PayrollConfig",
                table: "PayrollConfig",
                column: "Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PayrollConfig",
                table: "PayrollConfig");

            migrationBuilder.RenameTable(
                name: "PayrollConfig",
                newName: "InsuranceStatus");

            migrationBuilder.RenameIndex(
                name: "IX_PayrollConfig_Key",
                table: "InsuranceStatus",
                newName: "IX_InsuranceStatus_Key");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsuranceStatus",
                table: "InsuranceStatus",
                column: "Key");
        }
    }
}
