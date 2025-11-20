using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchedTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchedTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContractId = table.Column<int>(type: "integer", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    TransactionContent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BankBrandName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MatchedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchedTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchedTransactions_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MatchedTransactions_Users_MatchedByUserId",
                        column: x => x.MatchedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchedTransactions_ContractId",
                table: "MatchedTransactions",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchedTransactions_MatchedAt",
                table: "MatchedTransactions",
                column: "MatchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MatchedTransactions_MatchedByUserId",
                table: "MatchedTransactions",
                column: "MatchedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchedTransactions_ReferenceNumber",
                table: "MatchedTransactions",
                column: "ReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MatchedTransactions_Status",
                table: "MatchedTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MatchedTransactions_TransactionDate",
                table: "MatchedTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_MatchedTransactions_TransactionId",
                table: "MatchedTransactions",
                column: "TransactionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchedTransactions");
        }
    }
}
