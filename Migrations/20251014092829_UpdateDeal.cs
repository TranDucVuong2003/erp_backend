using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Users_AssignedTo",
                table: "Deals");

            migrationBuilder.DropForeignKey(
                name: "FK_Deals_Users_CreatedBy",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_AssignedTo",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_CreatedBy",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_Priority",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_Stage",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "ActualCloseDate",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "ExpectedCloseDate",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Deals");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "Deals");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Deals",
                newName: "Services");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_Probability",
                table: "Deals",
                column: "Probability");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_Value",
                table: "Deals",
                column: "Value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deals_Probability",
                table: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Deals_Value",
                table: "Deals");

            migrationBuilder.RenameColumn(
                name: "Services",
                table: "Deals",
                newName: "CreatedBy");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ActualCloseDate",
                table: "Deals",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedTo",
                table: "Deals",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpectedCloseDate",
                table: "Deals",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Deals",
                type: "text",
                nullable: false,
                defaultValue: "Medium");

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "Deals",
                type: "text",
                nullable: false,
                defaultValue: "Lead");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_AssignedTo",
                table: "Deals",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_CreatedBy",
                table: "Deals",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_Priority",
                table: "Deals",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_Stage",
                table: "Deals",
                column: "Stage");

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Users_AssignedTo",
                table: "Deals",
                column: "AssignedTo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Deals_Users_CreatedBy",
                table: "Deals",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
