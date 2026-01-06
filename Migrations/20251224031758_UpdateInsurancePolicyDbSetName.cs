using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInsurancePolicyDbSetName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Insurances",
                table: "Insurances");

            migrationBuilder.RenameTable(
                name: "Insurances",
                newName: "InsurancePolicy");

            migrationBuilder.RenameIndex(
                name: "IX_Insurances_Name",
                table: "InsurancePolicy",
                newName: "IX_InsurancePolicy_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Insurances_Code",
                table: "InsurancePolicy",
                newName: "IX_InsurancePolicy_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsurancePolicy",
                table: "InsurancePolicy",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_InsurancePolicy",
                table: "InsurancePolicy");

            migrationBuilder.RenameTable(
                name: "InsurancePolicy",
                newName: "Insurances");

            migrationBuilder.RenameIndex(
                name: "IX_InsurancePolicy_Name",
                table: "Insurances",
                newName: "IX_Insurances_Name");

            migrationBuilder.RenameIndex(
                name: "IX_InsurancePolicy_Code",
                table: "Insurances",
                newName: "IX_Insurances_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Insurances",
                table: "Insurances",
                column: "Id");
        }
    }
}
