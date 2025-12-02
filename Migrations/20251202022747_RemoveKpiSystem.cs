using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveKpiSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KpiCommissionTiers");

            migrationBuilder.DropTable(
                name: "KpiRecords");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "MarketingExpenses");

            migrationBuilder.DropTable(
                name: "UserKpiAssignments");

            migrationBuilder.DropTable(
                name: "MarketingBudgets");

            migrationBuilder.DropTable(
                name: "KPIs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KPIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    CalculationFormula = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CommissionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    KpiType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MeasurementUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPIs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPIs_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KPIs_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssignedToUserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    AcquisitionCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Campaign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsConverted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QualityScore = table.Column<int>(type: "integer", nullable: false),
                    ROI = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RevenueGenerated = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leads_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Leads_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Leads_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketingBudgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ActualROI = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualSpending = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsOverBudget = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OverBudgetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetROI = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsagePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingBudgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingBudgets_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MarketingBudgets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KpiCommissionTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KpiId = table.Column<int>(type: "integer", nullable: false),
                    CommissionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MinRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TierLevel = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiCommissionTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiCommissionTiers_KPIs_KpiId",
                        column: x => x.KpiId,
                        principalTable: "KPIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KpiRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    KpiId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AchievementPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ActualSpending = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ActualValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AverageResolutionTime = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    BudgetScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    BudgetUsagePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CommissionTierLevel = table.Column<int>(type: "integer", nullable: true),
                    CompletedTickets = table.Column<int>(type: "integer", nullable: true),
                    ConvertedLeads = table.Column<int>(type: "integer", nullable: true),
                    CostPerConversion = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CostPerLead = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsOverBudget = table.Column<bool>(type: "boolean", nullable: true),
                    LeadConversionRate = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    LeadsScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    MarketingTotalScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QualifiedLeads = table.Column<int>(type: "integer", nullable: true),
                    ROI = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RecordDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalLeads = table.Column<int>(type: "integer", nullable: true),
                    TotalTickets = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpiRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpiRecords_KPIs_KpiId",
                        column: x => x.KpiId,
                        principalTable: "KPIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KpiRecords_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KpiRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserKpiAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssignedBy = table.Column<int>(type: "integer", nullable: true),
                    KpiId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CustomTargetValue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserKpiAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserKpiAssignments_KPIs_KpiId",
                        column: x => x.KpiId,
                        principalTable: "KPIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserKpiAssignments_Users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserKpiAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MarketingExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    MarketingBudgetId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CostPerLead = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpenseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LeadsGenerated = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RevenueGenerated = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketingExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketingExpenses_MarketingBudgets_MarketingBudgetId",
                        column: x => x.MarketingBudgetId,
                        principalTable: "MarketingBudgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MarketingExpenses_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MarketingExpenses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KpiCommissionTiers_IsActive",
                table: "KpiCommissionTiers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KpiCommissionTiers_KpiId_TierLevel",
                table: "KpiCommissionTiers",
                columns: new[] { "KpiId", "TierLevel" });

            migrationBuilder.CreateIndex(
                name: "IX_KpiRecords_ApprovedBy",
                table: "KpiRecords",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_KpiRecords_KpiId_UserId_Period",
                table: "KpiRecords",
                columns: new[] { "KpiId", "UserId", "Period" });

            migrationBuilder.CreateIndex(
                name: "IX_KpiRecords_RecordDate",
                table: "KpiRecords",
                column: "RecordDate");

            migrationBuilder.CreateIndex(
                name: "IX_KpiRecords_Status",
                table: "KpiRecords",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_KpiRecords_UserId",
                table: "KpiRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_KPIs_CreatedAt",
                table: "KPIs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_KPIs_CreatedBy",
                table: "KPIs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_KPIs_DepartmentId",
                table: "KPIs",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_KPIs_IsActive",
                table: "KPIs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_KPIs_KpiType",
                table: "KPIs",
                column: "KpiType");

            migrationBuilder.CreateIndex(
                name: "IX_KPIs_Name",
                table: "KPIs",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_AssignedToUserId",
                table: "Leads",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CreatedAt",
                table: "Leads",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CreatedByUserId",
                table: "Leads",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CustomerId",
                table: "Leads",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_IsConverted",
                table: "Leads",
                column: "IsConverted");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_QualityScore",
                table: "Leads",
                column: "QualityScore");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Source",
                table: "Leads",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Status",
                table: "Leads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingBudgets_ApprovedBy",
                table: "MarketingBudgets",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingBudgets_Status",
                table: "MarketingBudgets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingBudgets_UserId_Period",
                table: "MarketingBudgets",
                columns: new[] { "UserId", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketingExpenses_ApprovedBy",
                table: "MarketingExpenses",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingExpenses_ExpenseDate",
                table: "MarketingExpenses",
                column: "ExpenseDate");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingExpenses_ExpenseType",
                table: "MarketingExpenses",
                column: "ExpenseType");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingExpenses_MarketingBudgetId",
                table: "MarketingExpenses",
                column: "MarketingBudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingExpenses_Status",
                table: "MarketingExpenses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MarketingExpenses_UserId",
                table: "MarketingExpenses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserKpiAssignments_AssignedBy",
                table: "UserKpiAssignments",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserKpiAssignments_AssignedDate",
                table: "UserKpiAssignments",
                column: "AssignedDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserKpiAssignments_KpiId",
                table: "UserKpiAssignments",
                column: "KpiId");

            migrationBuilder.CreateIndex(
                name: "IX_UserKpiAssignments_UserId_KpiId_IsActive",
                table: "UserKpiAssignments",
                columns: new[] { "UserId", "KpiId", "IsActive" });
        }
    }
}
