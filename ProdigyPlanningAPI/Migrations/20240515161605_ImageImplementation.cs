using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProdigyPlanningAPI.Migrations
{
    /// <inheritdoc />
    public partial class ImageImplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "event_banners",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    event_id = table.Column<int>(type: "int", nullable: true),
                    event_image = table.Column<byte[]>(type: "image", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_banners", x => x.id);
                    table.ForeignKey(
                        name: "FK_event_banners_events_id",
                        column: x => x.id,
                        principalTable: "events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "event_banners");
        }
    }
}
