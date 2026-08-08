using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTrakerApi.Migrations
{
    /// <inheritdoc />
    public partial class Updateabitsable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExecutionDayOfMonth",
                table: "Habits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExecutionDayOfWeek",
                table: "Habits",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionDayOfMonth",
                table: "Habits");

            migrationBuilder.DropColumn(
                name: "ExecutionDayOfWeek",
                table: "Habits");
        }
    }
}
