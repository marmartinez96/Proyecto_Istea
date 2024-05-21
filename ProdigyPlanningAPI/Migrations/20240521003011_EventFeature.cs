using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProdigyPlanningAPI.Migrations
{
    /// <inheritdoc />
    public partial class EventFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_featured",
                table: "events",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_featured",
                table: "events");
        }
    }
}
