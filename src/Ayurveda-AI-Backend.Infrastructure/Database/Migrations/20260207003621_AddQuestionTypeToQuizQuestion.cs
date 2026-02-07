using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ayurveda_AI_Backend.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionTypeToQuizQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuestionType",
                table: "QuizQuestions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionType",
                table: "QuizQuestions");
        }
    }
}
