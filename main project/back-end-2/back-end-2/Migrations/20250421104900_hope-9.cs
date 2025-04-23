using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end_2.Migrations
{
    /// <inheritdoc />
    public partial class hope9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TeamCommonAnswers_TeamId",
                table: "TeamCommonAnswers",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamCommonAnswers_Teams_TeamId",
                table: "TeamCommonAnswers",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamCommonAnswers_Teams_TeamId",
                table: "TeamCommonAnswers");

            migrationBuilder.DropIndex(
                name: "IX_TeamCommonAnswers_TeamId",
                table: "TeamCommonAnswers");
        }
    }
}
