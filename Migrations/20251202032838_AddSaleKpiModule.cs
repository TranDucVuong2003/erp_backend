using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleKpiModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommissionRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MinAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TierLevel = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaleKpiTargets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleKpiTargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleKpiTargets_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleKpiTargets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaleKpiRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    KpiTargetId = table.Column<int>(type: "integer", nullable: true),
                    TotalPaidAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AchievementPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsKpiAchieved = table.Column<bool>(type: "boolean", nullable: false),
                    CommissionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CommissionTierLevel = table.Column<int>(type: "integer", nullable: true),
                    TotalContracts = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleKpiRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleKpiRecords_SaleKpiTargets_KpiTargetId",
                        column: x => x.KpiTargetId,
                        principalTable: "SaleKpiTargets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SaleKpiRecords_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SaleKpiRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRates_IsActive",
                table: "CommissionRates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRates_MinAmount_MaxAmount",
                table: "CommissionRates",
                columns: new[] { "MinAmount", "MaxAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRates_TierLevel",
                table: "CommissionRates",
                column: "TierLevel");

            migrationBuilder.CreateIndex(
                name: "IX_SaleKpiRecords_ApprovedBy",
                table: "SaleKpiRecords",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SaleKpiRecords_IsKpiAchieved",
                table: "SaleKpiRecords",
                column: "IsKpiAchieved");

            migrationBuilder.CreateIndex(
                name: "IX_SaleKpiRecords_KpiTargetId",
                table: "SaleKpiRecords",
                column: "KpiTargetId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleKpiRecords_UserId_Month_Year",
                table: "SaleKpiRecords",
                columns: new[] { "UserId", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleKpiTargets_AssignedAt",
                table: "SaleKpiTargets",
                column: "AssignedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SaleKpiTargets_AssignedByUserId",
                table: "SaleKpiTargets",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleKpiTargets_IsActive",
                table: "SaleKpiTargets",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SaleKpiTargets_UserId_Month_Year",
                table: "SaleKpiTargets",
                columns: new[] { "UserId", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommissionRates");

            migrationBuilder.DropTable(
                name: "SaleKpiRecords");

            migrationBuilder.DropTable(
                name: "SaleKpiTargets");
        }
    }
}
