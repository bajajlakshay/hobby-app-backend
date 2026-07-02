using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HobbyApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmailOtpAttempts",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailOtpExpiresAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailOtpHash",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailOtpAttempts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailOtpExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailOtpHash",
                table: "AspNetUsers");
        }
    }
}
