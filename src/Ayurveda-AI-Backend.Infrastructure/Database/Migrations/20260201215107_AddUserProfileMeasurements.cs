using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ayurveda_AI_Backend.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HeightFeet",
                table: "UserProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HeightInches",
                table: "UserProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightLbs",
                table: "UserProfiles",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeightFeet",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "HeightInches",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "WeightLbs",
                table: "UserProfiles");
        }
    }
}
