using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MockMindAI.Api.Migrations
{
    /// <inheritdoc />
    public partial class PostgresInitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Department = table.Column<string>(type: "text", nullable: false),
                    CollegeName = table.Column<string>(type: "text", nullable: false),
                    AvatarKey = table.Column<string>(type: "text", nullable: false, defaultValue: "mentor"),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsDisabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InterviewAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Difficulty = table.Column<string>(type: "text", nullable: false),
                    Experience = table.Column<string>(type: "text", nullable: false),
                    InterviewType = table.Column<string>(type: "text", nullable: false),
                    IsTimedMode = table.Column<bool>(type: "boolean", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    WasAutoSubmitted = table.Column<bool>(type: "boolean", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    QuestionsJson = table.Column<string>(type: "text", nullable: false),
                    AnswersJson = table.Column<string>(type: "text", nullable: false),
                    SkillScoresJson = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
                    StrengthsJson = table.Column<string>(type: "text", nullable: false),
                    WeaknessesJson = table.Column<string>(type: "text", nullable: false),
                    ImprovedAnswer = table.Column<string>(type: "text", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewAttempts_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewAttempts_StudentId",
                table: "InterviewAttempts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewAttempts");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
