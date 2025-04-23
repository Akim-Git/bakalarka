using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back_end_2.Migrations
{
    /// <inheritdoc />
    public partial class hope4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAnswerCorrect",
                table: "Players",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAnswerCorrect",
                table: "Players");
        }
    }
}
