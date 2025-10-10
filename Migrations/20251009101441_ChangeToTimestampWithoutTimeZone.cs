using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToTimestampWithoutTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Đổi các cột DateTime từ timestamp with time zone sang timestamp without time zone
            migrationBuilder.Sql(@"
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""BirthDate"" TYPE timestamp without time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""EstablishedDate"" TYPE timestamp without time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""CreatedAt"" TYPE timestamp without time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""UpdatedAt"" TYPE timestamp without time zone;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: đổi lại thành timestamp with time zone
            migrationBuilder.Sql(@"
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""BirthDate"" TYPE timestamp with time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""EstablishedDate"" TYPE timestamp with time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""CreatedAt"" TYPE timestamp with time zone;
                
                ALTER TABLE ""Customers"" 
                ALTER COLUMN ""UpdatedAt"" TYPE timestamp with time zone;
            ");
        }
    }
}
