using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ayurveda_AI_Backend.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToHealthIndicator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "HealthIndicators",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30101"),
                column: "UserId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30102"),
                column: "UserId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30103"),
                column: "UserId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30104"),
                column: "UserId",
                value: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "HealthIndicators");
        }
    }
}
