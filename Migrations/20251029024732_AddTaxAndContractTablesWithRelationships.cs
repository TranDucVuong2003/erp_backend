using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxAndContractTablesWithRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaxId",
                table: "SaleOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Taxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TaxCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Tax_Amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TaxType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "VAT"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApplicableFor = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ServiceId = table.Column<int>(type: "integer", nullable: true),
                    AddonsId = table.Column<int>(type: "integer", nullable: true),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    TaxId = table.Column<int>(type: "integer", nullable: true),
                    TaxAmount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Addons_AddonsId",
                        column: x => x.AddonsId,
                        principalTable: "Addons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Contracts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contracts_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Contracts_Taxes_TaxId",
                        column: x => x.TaxId,
                        principalTable: "Taxes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SaleOrders_TaxId",
                table: "SaleOrders",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_AddonsId",
                table: "Contracts",
                column: "AddonsId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CreatedAt",
                table: "Contracts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CustomerId",
                table: "Contracts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_Expiration",
                table: "Contracts",
                column: "Expiration");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ServiceId",
                table: "Contracts",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_Status",
                table: "Contracts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_TaxId",
                table: "Contracts",
                column: "TaxId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_UserId",
                table: "Contracts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_IsActive",
                table: "Taxes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_TaxCode",
                table: "Taxes",
                column: "TaxCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_TaxType",
                table: "Taxes",
                column: "TaxType");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOrders_Taxes_TaxId",
                table: "SaleOrders",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOrders_Taxes_TaxId",
                table: "SaleOrders");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Taxes");

            migrationBuilder.DropIndex(
                name: "IX_SaleOrders_TaxId",
                table: "SaleOrders");

            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "SaleOrders");
        }
    }
}
