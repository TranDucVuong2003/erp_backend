using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxIdColumnToServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm cột TaxId vào bảng Services
            migrationBuilder.AddColumn<int>(
                name: "TaxId",
                table: "Services",
                type: "integer",
                nullable: true);

            // Thêm cột TaxId vào bảng Addons
            migrationBuilder.AddColumn<int>(
                name: "TaxId",
                table: "Addons",
                type: "integer",
                nullable: true);

            // Tạo index cho Services.TaxId
            migrationBuilder.CreateIndex(
                name: "IX_Services_TaxId",
                table: "Services",
                column: "TaxId");

            // Tạo index cho Addons.TaxId
            migrationBuilder.CreateIndex(
                name: "IX_Addons_TaxId",
                table: "Addons",
                column: "TaxId");

            // Thêm Foreign Key cho Services.TaxId
            migrationBuilder.AddForeignKey(
                name: "FK_Services_Taxes_TaxId",
                table: "Services",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Thêm Foreign Key cho Addons.TaxId
            migrationBuilder.AddForeignKey(
                name: "FK_Addons_Taxes_TaxId",
                table: "Addons",
                column: "TaxId",
                principalTable: "Taxes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Xóa Foreign Key cho Services
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Taxes_TaxId",
                table: "Services");

            // Xóa Foreign Key cho Addons
            migrationBuilder.DropForeignKey(
                name: "FK_Addons_Taxes_TaxId",
                table: "Addons");

            // Xóa index cho Services
            migrationBuilder.DropIndex(
                name: "IX_Services_TaxId",
                table: "Services");

            // Xóa index cho Addons
            migrationBuilder.DropIndex(
                name: "IX_Addons_TaxId",
                table: "Addons");

            // Xóa cột TaxId khỏi Services
            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Services");

            // Xóa cột TaxId khỏi Addons
            migrationBuilder.DropColumn(
                name: "TaxId",
                table: "Addons");
        }
    }
}
