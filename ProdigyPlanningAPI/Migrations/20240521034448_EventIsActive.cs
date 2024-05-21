using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProdigyPlanningAPI.Migrations
{
    /// <inheritdoc />
    public partial class EventIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "is_active",
                table: "events",
                type: "nvarchar(max)",
                nullable: false,
                computedColumnSql: "CASE WHEN date >= GETDATE() THEN 1 ELSE 0 END",
                stored: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_active",
                table: "events");
        }
    }
}
