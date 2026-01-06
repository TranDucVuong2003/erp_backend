using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceAndFamilyDeductionToPayslip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AssessableIncome",
                table: "Payslips",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FamilyDeduction",
                table: "Payslips",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceDeduction",
                table: "Payslips",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssessableIncome",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "FamilyDeduction",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "InsuranceDeduction",
                table: "Payslips");
        }
    }
}
