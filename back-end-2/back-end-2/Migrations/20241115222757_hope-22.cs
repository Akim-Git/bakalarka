using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end_2.Migrations
{
    /// <inheritdoc />
    public partial class hope22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lobbies_Quizzes_QuizId",
                table: "Lobbies");

            migrationBuilder.DropIndex(
                name: "IX_Lobbies_QuizId",
                table: "Lobbies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Lobbies_QuizId",
                table: "Lobbies",
                column: "QuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lobbies_Quizzes_QuizId",
                table: "Lobbies",
                column: "QuizId",
                principalTable: "Quizzes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
