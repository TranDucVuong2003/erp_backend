using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class SeedTaxBracketsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payslips_Insurances_InsuranceId",
                table: "Payslips");

            migrationBuilder.DropForeignKey(
                name: "FK_SalaryBases_Insurances_InsuranceId",
                table: "SalaryBases");

            migrationBuilder.DropIndex(
                name: "IX_SalaryBases_InsuranceId",
                table: "SalaryBases");

            migrationBuilder.DropIndex(
                name: "IX_Payslips_InsuranceId",
                table: "Payslips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsuranceStatus",
                table: "InsuranceStatus");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceStatus_Status",
                table: "InsuranceStatus");

            migrationBuilder.DropColumn(
                name: "InsuranceId",
                table: "SalaryBases");

            migrationBuilder.DropColumn(
                name: "InsuranceId",
                table: "Payslips");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "InsuranceStatus");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "InsuranceStatus");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InsuranceStatus");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InsuranceStatus");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Insurances");

            migrationBuilder.RenameColumn(
                name: "Rate",
                table: "Insurances",
                newName: "EmployerRate");

            migrationBuilder.RenameColumn(
                name: "NameInsurance",
                table: "Insurances",
                newName: "Name");

            migrationBuilder.RenameIndex(
                name: "IX_Insurances_NameInsurance",
                table: "Insurances",
                newName: "IX_Insurances_Name");

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseSalary",
                table: "SalaryBases",
                type: "numeric(18,0)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddColumn<string>(
                name: "ContractType",
                table: "SalaryBases",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "OFFICIAL");

            migrationBuilder.AddColumn<int>(
                name: "DependentsCount",
                table: "SalaryBases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasCommitment08",
                table: "SalaryBases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceSalary",
                table: "SalaryBases",
                type: "numeric(18,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsStandardInsuranceMode",
                table: "SalaryBases",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "InsuranceStatus",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InsuranceStatus",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "InsuranceStatus",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CapBaseType",
                table: "Insurances",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Insurances",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<float>(
                name: "EmployeeRate",
                table: "Insurances",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsuranceStatus",
                table: "InsuranceStatus",
                column: "Key");

            migrationBuilder.CreateTable(
                name: "TaxBrackets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MinIncome = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxIncome = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    TaxRate = table.Column<float>(type: "real", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxBrackets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBases_ContractType",
                table: "SalaryBases",
                column: "ContractType");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceStatus_Key",
                table: "InsuranceStatus",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Insurances_Code",
                table: "Insurances",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxBrackets_MinIncome_MaxIncome",
                table: "TaxBrackets",
                columns: new[] { "MinIncome", "MaxIncome" });

            migrationBuilder.CreateIndex(
                name: "IX_TaxBrackets_TaxRate",
                table: "TaxBrackets",
                column: "TaxRate");

            // ✅ Seed default TaxBrackets data
            migrationBuilder.InsertData(
                table: "TaxBrackets",
                columns: new[] { "MinIncome", "MaxIncome", "TaxRate", "Notes", "CreatedAt" },
                values: new object[,]
                {
                    { 0m, 30000000m, 0.1f, "Bậc 1: Thu nhập từ 0 đến 30 triệu", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { 30000001m, 60000000m, 0.20f, "Bậc 2: Thu nhập từ 30 đến 60 triệu", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { 60000001m, 100000000m, 0.3f, "Bậc 3: Thu nhập từ 60 đến 100 triệu", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                    { 100000001m, null, 0.35f, "Bậc 4: Thu nhập trên 100 triệu", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxBrackets");

            migrationBuilder.DropIndex(
                name: "IX_SalaryBases_ContractType",
                table: "SalaryBases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InsuranceStatus",
                table: "InsuranceStatus");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceStatus_Key",
                table: "InsuranceStatus");

            migrationBuilder.DropIndex(
                name: "IX_Insurances_Code",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "ContractType",
                table: "SalaryBases");

            migrationBuilder.DropColumn(
                name: "DependentsCount",
                table: "SalaryBases");

            migrationBuilder.DropColumn(
                name: "HasCommitment08",
                table: "SalaryBases");

            migrationBuilder.DropColumn(
                name: "InsuranceSalary",
                table: "SalaryBases");

            migrationBuilder.DropColumn(
                name: "IsStandardInsuranceMode",
                table: "SalaryBases");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "InsuranceStatus");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "InsuranceStatus");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "InsuranceStatus");

            migrationBuilder.DropColumn(
                name: "CapBaseType",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Insurances");

            migrationBuilder.DropColumn(
                name: "EmployeeRate",
                table: "Insurances");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Insurances",
                newName: "NameInsurance");

            migrationBuilder.RenameColumn(
                name: "EmployerRate",
                table: "Insurances",
                newName: "Rate");

            migrationBuilder.RenameIndex(
                name: "IX_Insurances_Name",
                table: "Insurances",
                newName: "IX_Insurances_NameInsurance");

            migrationBuilder.AlterColumn<decimal>(
                name: "BaseSalary",
                table: "SalaryBases",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,0)");

            migrationBuilder.AddColumn<int>(
                name: "InsuranceId",
                table: "SalaryBases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InsuranceId",
                table: "Payslips",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "InsuranceStatus",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "InsuranceStatus",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "InsuranceStatus",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "InsuranceStatus",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Insurances",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_InsuranceStatus",
                table: "InsuranceStatus",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryBases_InsuranceId",
                table: "SalaryBases",
                column: "InsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_InsuranceId",
                table: "Payslips",
                column: "InsuranceId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceStatus_Status",
                table: "InsuranceStatus",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Payslips_Insurances_InsuranceId",
                table: "Payslips",
                column: "InsuranceId",
                principalTable: "Insurances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryBases_Insurances_InsuranceId",
                table: "SalaryBases",
                column: "InsuranceId",
                principalTable: "Insurances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
