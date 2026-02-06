using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ayurveda_AI_Backend.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyHealthIndicator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "HealthIndicators");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "HealthIndicators",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "HealthIndicators",
                newName: "Indication");

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30101"),
                columns: new[] { "Indication", "Value" },
                values: new object[] { "Digestion", "Good" });

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30102"),
                columns: new[] { "Indication", "Value" },
                values: new object[] { "Sleep Quality", "Good" });

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30103"),
                columns: new[] { "Indication", "Value" },
                values: new object[] { "Stress", "Low" });

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30104"),
                columns: new[] { "Indication", "Value" },
                values: new object[] { "Energy", "High" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "HealthIndicators",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Indication",
                table: "HealthIndicators",
                newName: "Description");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "HealthIndicators",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30101"),
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "Digestive", "Bloating, appetite, regularity.", "Digestion" });

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30102"),
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "Sleep", "Restful sleep and duration.", "Sleep Quality" });

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30103"),
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "Mind", "Mental tension and calmness.", "Stress" });

            migrationBuilder.UpdateData(
                table: "HealthIndicators",
                keyColumn: "Id",
                keyValue: new Guid("6a6165af-2a69-4c6a-b8d1-4d8f9aa30104"),
                columns: new[] { "Category", "Description", "Name" },
                values: new object[] { "Energy", "Daily vitality.", "Energy" });
        }
    }
}
