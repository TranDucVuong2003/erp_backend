using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyAndUrlTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Users_UserId",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "Link",
                table: "Urls",
                newName: "Links");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Urls",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_Urls_CreatedAt",
                table: "Urls",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Urls_Links",
                table: "Urls",
                column: "Links");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CreatedAt",
                table: "Companies",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_Mst",
                table: "Companies",
                column: "Mst",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TenDoanhNghiep",
                table: "Companies",
                column: "TenDoanhNghiep");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_TrangThai",
                table: "Companies",
                column: "TrangThai");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Users_UserId",
                table: "Companies",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Users_UserId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Urls_CreatedAt",
                table: "Urls");

            migrationBuilder.DropIndex(
                name: "IX_Urls_Links",
                table: "Urls");

            migrationBuilder.DropIndex(
                name: "IX_Companies_CreatedAt",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_Mst",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_TenDoanhNghiep",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_TrangThai",
                table: "Companies");

            migrationBuilder.RenameColumn(
                name: "Links",
                table: "Urls",
                newName: "Link");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Urls",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Companies",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Users_UserId",
                table: "Companies",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
