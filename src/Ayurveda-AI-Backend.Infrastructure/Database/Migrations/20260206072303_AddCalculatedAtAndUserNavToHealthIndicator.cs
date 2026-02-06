using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ayurveda_AI_Backend.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculatedAtAndUserNavToHealthIndicator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30101"));

            migrationBuilder.DeleteData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30102"));

            migrationBuilder.DeleteData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30103"));

            migrationBuilder.DeleteData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30104"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CalculatedAt",
                table: "HealthIndicators",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_HealthIndicators_UserId",
                table: "HealthIndicators",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthIndicators_Users_UserId",
                table: "HealthIndicators",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthIndicators_Users_UserId",
                table: "HealthIndicators");

            migrationBuilder.DropIndex(
                name: "IX_HealthIndicators_UserId",
                table: "HealthIndicators");

            migrationBuilder.DropColumn(
                name: "CalculatedAt",
                table: "HealthIndicators");

            migrationBuilder.InsertData(
                table: "HealthIndicators",
                columns: new[] { "Id", "Indication", "IsActive", "UserId", "Value" },
                values: new object[,]
                {
                    { new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30101"), "Digestion", true, new Guid("00000000-0000-0000-0000-000000000000"), "Good" },
                    { new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30102"), "Sleep Quality", true, new Guid("00000000-0000-0000-0000-000000000000"), "Good" },
                    { new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30103"), "Stress", true, new Guid("00000000-0000-0000-0000-000000000000"), "Low" },
                    { new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30104"), "Energy", true, new Guid("00000000-0000-0000-0000-000000000000"), "High" }
                });
        }
    }
}
