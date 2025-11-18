using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserToCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Customers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedByUserId",
                table: "Customers",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_CreatedByUserId",
                table: "Customers",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_CreatedByUserId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CreatedByUserId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Customers");
        }
    }
}
