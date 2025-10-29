using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleOrderServicesAndAddons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "SaleOrders",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "SaleOrderAddons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleOrderId = table.Column<int>(type: "integer", nullable: false),
                    AddonId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UnitPrice = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleOrderAddons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleOrderAddons_Addons_AddonId",
                        column: x => x.AddonId,
                        principalTable: "Addons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleOrderAddons_SaleOrders_SaleOrderId",
                        column: x => x.SaleOrderId,
                        principalTable: "SaleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleOrderServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SaleOrderId = table.Column<int>(type: "integer", nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    UnitPrice = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleOrderServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleOrderServices_SaleOrders_SaleOrderId",
                        column: x => x.SaleOrderId,
                        principalTable: "SaleOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleOrderServices_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderAddons_AddonId",
                table: "SaleOrderAddons",
                column: "AddonId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderAddons_SaleOrderId",
                table: "SaleOrderAddons",
                column: "SaleOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderAddons_SaleOrderId_AddonId",
                table: "SaleOrderAddons",
                columns: new[] { "SaleOrderId", "AddonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderServices_SaleOrderId",
                table: "SaleOrderServices",
                column: "SaleOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderServices_SaleOrderId_ServiceId",
                table: "SaleOrderServices",
                columns: new[] { "SaleOrderId", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrderServices_ServiceId",
                table: "SaleOrderServices",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaleOrderAddons");

            migrationBuilder.DropTable(
                name: "SaleOrderServices");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "SaleOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
