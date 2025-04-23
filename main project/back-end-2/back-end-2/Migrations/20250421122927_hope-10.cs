using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end_2.Migrations
{
    /// <inheritdoc />
    public partial class hope10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LobbyId",
                table: "TeamCommonAnswers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LobbyId",
                table: "TeamCommonAnswers");
        }
    }
}
