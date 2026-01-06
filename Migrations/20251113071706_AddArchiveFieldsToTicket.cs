using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveFieldsToTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArchivedById",
                table: "Tickets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Tickets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ArchivedById",
                table: "Tickets",
                column: "ArchivedById");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_IsArchived",
                table: "Tickets",
                column: "IsArchived");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Users_ArchivedById",
                table: "Tickets",
                column: "ArchivedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Users_ArchivedById",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_ArchivedById",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_IsArchived",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ArchivedById",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Tickets");
        }
    }
}
