using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace erp_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJwtTokenFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceInfo",
                table: "JwtTokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "JwtTokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "JwtTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "JwtTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReasonRevoked",
                table: "JwtTokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplacedByToken",
                table: "JwtTokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "JwtTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "JwtTokens",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceInfo",
                table: "JwtTokens");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "JwtTokens");

            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "JwtTokens");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "JwtTokens");

            migrationBuilder.DropColumn(
                name: "ReasonRevoked",
                table: "JwtTokens");

            migrationBuilder.DropColumn(
                name: "ReplacedByToken",
                table: "JwtTokens");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "JwtTokens");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "JwtTokens");
        }
    }
}
