using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MockMindAI.Api.Migrations
{
    /// <inheritdoc />
    public partial class FeatureExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarKey",
                table: "Students",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "mentor");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "InterviewAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTimedMode",
                table: "InterviewAttempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SkillScoresJson",
                table: "InterviewAttempts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<bool>(
                name: "WasAutoSubmitted",
                table: "InterviewAttempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE Students
                SET IsAdmin = 1
                WHERE Id = (SELECT MIN(Id) FROM Students)
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarKey",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "InterviewAttempts");

            migrationBuilder.DropColumn(
                name: "IsTimedMode",
                table: "InterviewAttempts");

            migrationBuilder.DropColumn(
                name: "SkillScoresJson",
                table: "InterviewAttempts");

            migrationBuilder.DropColumn(
                name: "WasAutoSubmitted",
                table: "InterviewAttempts");
        }
    }
}
