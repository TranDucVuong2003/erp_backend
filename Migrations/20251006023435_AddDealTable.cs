using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddDealTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    ExpectedCloseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ActualCloseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Priority = table.Column<string>(type: "text", nullable: false, defaultValue: "Medium"),
                    Probability = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Stage = table.Column<string>(type: "text", nullable: false, defaultValue: "Lead"),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AssignedTo = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deals_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deals_Users_AssignedTo",
                        column: x => x.AssignedTo,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Deals_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deals_AssignedTo",
                table: "Deals",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CreatedBy",
                table: "Deals",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CustomerId",
                table: "Deals",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_Priority",
                table: "Deals",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_Stage",
                table: "Deals",
                column: "Stage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deals");
        }
    }
}
