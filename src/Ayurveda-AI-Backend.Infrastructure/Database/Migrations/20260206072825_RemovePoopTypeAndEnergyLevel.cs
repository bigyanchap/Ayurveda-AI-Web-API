using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ayurveda_AI_Backend.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemovePoopTypeAndEnergyLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnergyLevels");

            migrationBuilder.DropTable(
                name: "PoopTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnergyLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PoopTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoopTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "EnergyLevels",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("b1f0f611-9a8d-4d6b-9b1c-9af1c8f10001"), "Exhausted or drained.", "Very Low" },
                    { new Guid("b1f0f611-9a8d-4d6b-9b1c-9af1c8f10002"), "Below usual energy.", "Low" },
                    { new Guid("b1f0f611-9a8d-4d6b-9b1c-9af1c8f10003"), "Stable energy.", "Moderate" },
                    { new Guid("b1f0f611-9a8d-4d6b-9b1c-9af1c8f10004"), "Energetic and focused.", "High" }
                });

            migrationBuilder.InsertData(
                table: "PoopTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("0f9b7d5a-0b78-4b44-8d9a-4e2c2c5f6d44"), "Smooth and soft, ideal.", "Type 4" },
                    { new Guid("2a0d8b4c-0c6f-4ea1-9f90-4f3c8f4e7b22"), "Sausage-shaped but lumpy.", "Type 2" },
                    { new Guid("a8b9c2d3-3c2d-4f4b-8e7f-2b4c6d8e9f55"), "Soft blobs, clear-cut.", "Type 5" },
                    { new Guid("c8f5b8e7-6f5b-4c4a-9b90-8e1a0f7d9c33"), "Cracked surface, normal.", "Type 3" },
                    { new Guid("f2e2c9a1-4d6b-4abf-9c55-3c2d0e2b1b11"), "Hard lumps, difficult to pass.", "Type 1" }
                });
        }
    }
}
