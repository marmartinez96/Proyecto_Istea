using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProdigyPlanningAPI.Migrations
{
    /// <inheritdoc />
    public partial class Securityquestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "security_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_security_questions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    answer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_questions_security_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "security_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_questions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_questions_question_id",
                table: "user_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_questions_user_id",
                table: "user_questions",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_questions");

            migrationBuilder.DropTable(
                name: "security_questions");

            migrationBuilder.AlterColumn<string>(
                name: "is_active",
                table: "events",
                type: "nvarchar(max)",
                nullable: false,
                computedColumnSql: "CASE WHEN date >= GETDATE() THEN 1 ELSE 0 END",
                stored: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldComputedColumnSql: "CASE WHEN date >= GETDATE() THEN 1 ELSE 0 END",
                oldStored: false);
        }
    }
}
