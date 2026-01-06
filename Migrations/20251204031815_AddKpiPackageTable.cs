using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddKpiPackageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CommissionRates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CommissionRates",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "CommissionRates",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CommissionRates",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SaleKpiTargets",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<int>(
                name: "KpiPackageId",
                table: "SaleKpiTargets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "KpiPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiPackages_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleKpiTargets_KpiPackageId",
                table: "SaleKpiTargets",
                column: "KpiPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiPackages_CreatedByUserId",
                table: "KpiPackages",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KpiPackages_IsActive",
                table: "KpiPackages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KpiPackages_Month_Year",
                table: "KpiPackages",
                columns: new[] { "Month", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_KpiPackages_Name",
                table: "KpiPackages",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleKpiTargets_KpiPackages_KpiPackageId",
                table: "SaleKpiTargets",
                column: "KpiPackageId",
                principalTable: "KpiPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleKpiTargets_KpiPackages_KpiPackageId",
                table: "SaleKpiTargets");

            migrationBuilder.DropTable(
                name: "KpiPackages");

            migrationBuilder.DropIndex(
                name: "IX_SaleKpiTargets_KpiPackageId",
                table: "SaleKpiTargets");

            migrationBuilder.DropColumn(
                name: "KpiPackageId",
                table: "SaleKpiTargets");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "SaleKpiTargets",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.InsertData(
                table: "CommissionRates",
                columns: new[] { "Id", "CommissionPercentage", "CreatedAt", "Description", "IsActive", "MaxAmount", "MinAmount", "TierLevel", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 5.00m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "15 triệu - 30 triệu VND", true, 30000000m, 15000000m, 1, null },
                    { 2, 7.00m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "30 triệu - 60 triệu VND", true, 60000000m, 30000001m, 2, null },
                    { 3, 8.00m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "60 triệu - 100 triệu VND", true, 100000000m, 60000001m, 3, null },
                    { 4, 10.00m, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Trên 100 triệu VND", true, null, 100000001m, 4, null }
                });
        }
    }
}
