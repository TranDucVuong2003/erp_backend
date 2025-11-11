using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserForeignKeyToQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Users_CreatedByUserId",
                table: "Quotes");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Users_CreatedByUserId",
                table: "Quotes",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quotes_Users_CreatedByUserId",
                table: "Quotes");

            migrationBuilder.AddForeignKey(
                name: "FK_Quotes_Users_CreatedByUserId",
                table: "Quotes",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
