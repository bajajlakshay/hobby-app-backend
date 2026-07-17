using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HobbyApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderAtToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReminderAt",
                table: "Tasks",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderAt",
                table: "Tasks");
        }
    }
}
