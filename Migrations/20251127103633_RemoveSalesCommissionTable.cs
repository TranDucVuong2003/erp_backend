using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSalesCommissionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesCommissions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesCommissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    KpiId = table.Column<int>(type: "integer", nullable: true),
                    SaleOrderId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AchievementRate = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BaseTargetRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CommissionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PeriodMonth = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Revenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    TierLevel = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesCommissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesCommissions_KPIs_KpiId",
                        column: x => x.KpiId,
                        principalTable: "KPIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesCommissions_SaleOrders_SaleOrderId",
                        column: x => x.SaleOrderId,
                        principalTable: "SaleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesCommissions_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SalesCommissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesCommissions_ApprovedBy",
                table: "SalesCommissions",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCommissions_CreatedAt",
                table: "SalesCommissions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCommissions_KpiId",
                table: "SalesCommissions",
                column: "KpiId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCommissions_Period",
                table: "SalesCommissions",
                column: "Period");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCommissions_SaleOrderId",
                table: "SalesCommissions",
                column: "SaleOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCommissions_Status",
                table: "SalesCommissions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCommissions_TierLevel",
                table: "SalesCommissions",
                column: "TierLevel");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCommissions_UserId_PeriodMonth",
                table: "SalesCommissions",
                columns: new[] { "UserId", "PeriodMonth" });
        }
    }
}
