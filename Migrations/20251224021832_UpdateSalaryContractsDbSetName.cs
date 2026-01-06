using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSalaryContractsDbSetName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalaryBases_Users_UserId",
                table: "SalaryBases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SalaryBases",
                table: "SalaryBases");

            migrationBuilder.RenameTable(
                name: "SalaryBases",
                newName: "SalaryContracts");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryBases_UserId",
                table: "SalaryContracts",
                newName: "IX_SalaryContracts_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryBases_ContractType",
                table: "SalaryContracts",
                newName: "IX_SalaryContracts_ContractType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalaryContracts",
                table: "SalaryContracts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryContracts_Users_UserId",
                table: "SalaryContracts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalaryContracts_Users_UserId",
                table: "SalaryContracts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SalaryContracts",
                table: "SalaryContracts");

            migrationBuilder.RenameTable(
                name: "SalaryContracts",
                newName: "SalaryBases");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryContracts_UserId",
                table: "SalaryBases",
                newName: "IX_SalaryBases_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryContracts_ContractType",
                table: "SalaryBases",
                newName: "IX_SalaryBases_ContractType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SalaryBases",
                table: "SalaryBases",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryBases_Users_UserId",
                table: "SalaryBases",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
