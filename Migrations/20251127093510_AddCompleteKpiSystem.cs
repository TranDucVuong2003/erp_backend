using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCompleteKpiSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetPercentage",
                table: "KPIs");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "KPIs",
                newName: "KpiType");

            migrationBuilder.RenameIndex(
                name: "IX_KPIs_Department",
                table: "KPIs",
                newName: "IX_KPIs_KpiType");

            migrationBuilder.AlterColumn<decimal>(
                name: "TargetValue",
                table: "KPIs",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<string>(
                name: "CalculationFormula",
                table: "KPIs",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommissionType",
                table: "KPIs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "KPIs",
                type: "integer",
                nullable: true);

            // Step 1: Add DepartmentId as nullable first
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "KPIs",
                type: "integer",
                nullable: true);

            // Step 2: Delete any existing KPI records with invalid data or set to first valid department
            // Option 1: Delete invalid records
            migrationBuilder.Sql("DELETE FROM \"KPIs\" WHERE \"DepartmentId\" IS NULL OR \"DepartmentId\" = 0 OR \"DepartmentId\" NOT IN (SELECT \"Id\" FROM \"Departments\")");
            
            // Option 2: Or update to first available department (uncomment if preferred)
            // migrationBuilder.Sql(@"
            //     UPDATE ""KPIs"" 
            //     SET ""DepartmentId"" = (SELECT MIN(""Id"") FROM ""Departments"") 
            //     WHERE ""DepartmentId"" IS NULL OR ""DepartmentId"" = 0 OR ""DepartmentId"" NOT IN (SELECT ""Id"" FROM ""Departments"")
            // ");

            // Step 3: Make DepartmentId non-nullable
            migrationBuilder.AlterColumn<int>(
                name: "DepartmentId",
                table: "KPIs",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "KPIs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "KPIs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MeasurementUnit",
                table: "KPIs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Period",
                table: "KPIs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "KPIs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "KPIs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "KpiCommissionTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KpiId = table.Column<int>(type: "integer", nullable: false),
                    TierLevel = table.Column<int>(type: "integer", nullable: false),
                    MinRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MaxRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    KpiId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    ActualValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AchievementPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CommissionPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CommissionTierLevel = table.Column<int>(type: "integer", nullable: true),
                    TotalTickets = table.Column<int>(type: "integer", nullable: true),
                    CompletedTickets = table.Column<int>(type: "integer", nullable: true),
                    AverageResolutionTime = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    TotalLeads = table.Column<int>(type: "integer", nullable: true),
                    QualifiedLeads = table.Column<int>(type: "integer", nullable: true),
                    ConvertedLeads = table.Column<int>(type: "integer", nullable: true),
                    LeadConversionRate = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    LeadsScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    ApprovedBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ActualSpending = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    BudgetUsagePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    IsOverBudget = table.Column<bool>(type: "boolean", nullable: true),
                    BudgetScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    MarketingTotalScore = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CostPerLead = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CostPerConversion = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ROI = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Period = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RecordDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Campaign = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    QualityScore = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsConverted = table.Column<bool>(type: "boolean", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: true),
                    AssignedToUserId = table.Column<int>(type: "integer", nullable: true),
                    ConvertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevenueGenerated = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AcquisitionCost = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ROI = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Period = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    ApprovedBudget = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ActualSpending = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UsagePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsOverBudget = table.Column<bool>(type: "boolean", nullable: false),
                    OverBudgetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "UserKpiAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    KpiId = table.Column<int>(type: "integer", nullable: false),
                    CustomTargetValue = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedBy = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    MarketingBudgetId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ExpenseType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LeadsGenerated = table.Column<int>(type: "integer", nullable: true),
                    RevenueGenerated = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CostPerLead = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ApprovedBy = table.Column<int>(type: "integer", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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

            // Step 4: Add foreign key constraints after data is cleaned
            migrationBuilder.AddForeignKey(
                name: "FK_KPIs_Departments_DepartmentId",
                table: "KPIs",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_KPIs_Users_CreatedBy",
                table: "KPIs",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KPIs_Departments_DepartmentId",
                table: "KPIs");

            migrationBuilder.DropForeignKey(
                name: "FK_KPIs_Users_CreatedBy",
                table: "KPIs");

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

            migrationBuilder.DropIndex(
                name: "IX_KPIs_CreatedBy",
                table: "KPIs");

            migrationBuilder.DropIndex(
                name: "IX_KPIs_DepartmentId",
                table: "KPIs");

            migrationBuilder.DropIndex(
                name: "IX_KPIs_IsActive",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "CalculationFormula",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "CommissionType",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "MeasurementUnit",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "Period",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "KPIs");

            migrationBuilder.RenameColumn(
                name: "KpiType",
                table: "KPIs",
                newName: "Department");

            migrationBuilder.RenameIndex(
                name: "IX_KPIs_KpiType",
                table: "KPIs",
                newName: "IX_KPIs_Department");

            migrationBuilder.AlterColumn<float>(
                name: "TargetValue",
                table: "KPIs",
                type: "real",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "TargetPercentage",
                table: "KPIs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
