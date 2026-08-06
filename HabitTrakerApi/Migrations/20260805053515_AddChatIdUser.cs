using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTrakerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddChatIdUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChatId",
                table: "Users",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Users");
        }
    }
}
