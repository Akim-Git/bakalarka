using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end_2.Migrations
{
    /// <inheritdoc />
    public partial class hope13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_QuizTypes_QuizTypeId",
                table: "Quizzes");

            migrationBuilder.DropTable(
                name: "QuizTypes");

            migrationBuilder.DropIndex(
                name: "IX_Quizzes_QuizTypeId",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "QuizTypeId",
                table: "Quizzes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuizTypeId",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QuizTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_QuizTypeId",
                table: "Quizzes",
                column: "QuizTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_QuizTypes_QuizTypeId",
                table: "Quizzes",
                column: "QuizTypeId",
                principalTable: "QuizTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
