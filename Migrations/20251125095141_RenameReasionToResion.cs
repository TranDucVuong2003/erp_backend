using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameReasionToResion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Reasions_ResionId",
                table: "Departments");

            // Rename table from Reasions to Resions
            migrationBuilder.RenameTable(
                name: "Reasions",
                newName: "Resions");

            // Rename primary key
            migrationBuilder.RenameIndex(
                name: "PK_Reasions",
                table: "Resions",
                newName: "PK_Resions");

            // Rename index
            migrationBuilder.RenameIndex(
                name: "IX_Reasions_City",
                table: "Resions",
                newName: "IX_Resions_City");

            // Add foreign key constraint back
            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Resions_ResionId",
                table: "Departments",
                column: "ResionId",
                principalTable: "Resions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Resions_ResionId",
                table: "Departments");

            // Rename table back from Resions to Reasions
            migrationBuilder.RenameTable(
                name: "Resions",
                newName: "Reasions");

            // Rename primary key back
            migrationBuilder.RenameIndex(
                name: "PK_Resions",
                table: "Reasions",
                newName: "PK_Reasions");

            // Rename index back
            migrationBuilder.RenameIndex(
                name: "IX_Resions_City",
                table: "Reasions",
                newName: "IX_Reasions_City");

            // Add foreign key constraint back
            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Reasions_ResionId",
                table: "Departments",
                column: "ResionId",
                principalTable: "Reasions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
