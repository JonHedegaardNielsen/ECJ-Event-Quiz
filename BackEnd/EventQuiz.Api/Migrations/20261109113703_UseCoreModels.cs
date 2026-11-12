using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventQuiz.Api.Migrations
{
    /// <inheritdoc />
    public partial class UseCoreModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Quizzes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Questions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Participants",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "AnswerOptions",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "TimeLimitSeconds",
                table: "Questions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<double>(
                name: "AnsweredAfterSeconds",
                table: "ParticipantAnswers",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "ParticipantAnswers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RoomId",
                table: "ParticipantAnswers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantAnswers_RoomId",
                table: "ParticipantAnswers",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantAnswers_QuizSessions_RoomId",
                table: "ParticipantAnswers",
                column: "RoomId",
                principalTable: "QuizSessions",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParticipantAnswers_QuizSessions_RoomId",
                table: "ParticipantAnswers");

            migrationBuilder.DropIndex(
                name: "IX_ParticipantAnswers_RoomId",
                table: "ParticipantAnswers");

            migrationBuilder.DropColumn(
                name: "TimeLimitSeconds",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "AnsweredAfterSeconds",
                table: "ParticipantAnswers");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "ParticipantAnswers");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "ParticipantAnswers");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Quizzes",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Questions",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Participants",
                newName: "ID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AnswerOptions",
                newName: "ID");
        }
    }
}
