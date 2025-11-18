using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeNumberContractToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a temporary column for the integer values
            migrationBuilder.AddColumn<int>(
                name: "NumberContract_Temp",
                table: "Contracts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Step 2: Convert existing string values to integers
            // If the string value is a valid integer, convert it; otherwise, use the Id as fallback
            migrationBuilder.Sql(@"
                UPDATE ""Contracts""
                SET ""NumberContract_Temp"" = 
                    CASE 
                        WHEN ""NumberContract"" ~ '^[0-9]+$' 
                        THEN CAST(""NumberContract"" AS INTEGER)
                        ELSE ""Id""
                    END;
            ");

            // Step 3: Drop the old string column
            migrationBuilder.DropColumn(
                name: "NumberContract",
                table: "Contracts");

            // Step 4: Rename the temporary column to NumberContract
            migrationBuilder.RenameColumn(
                name: "NumberContract_Temp",
                table: "Contracts",
                newName: "NumberContract");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add a temporary string column
            migrationBuilder.AddColumn<string>(
                name: "NumberContract_Temp",
                table: "Contracts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            // Step 2: Convert integer values back to strings
            migrationBuilder.Sql(@"
                UPDATE ""Contracts""
                SET ""NumberContract_Temp"" = CAST(""NumberContract"" AS VARCHAR(100));
            ");

            // Step 3: Drop the integer column
            migrationBuilder.DropColumn(
                name: "NumberContract",
                table: "Contracts");

            // Step 4: Rename the temporary column back to NumberContract
            migrationBuilder.RenameColumn(
                name: "NumberContract_Temp",
                table: "Contracts",
                newName: "NumberContract");
        }
    }
}
