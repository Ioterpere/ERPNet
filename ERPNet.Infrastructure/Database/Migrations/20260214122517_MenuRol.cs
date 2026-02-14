using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class MenuRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Recursos_RecursoId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_RecursoId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "RecursoId",
                table: "Menus");

            migrationBuilder.CreateTable(
                name: "MenusRoles",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenusRoles", x => new { x.MenuId, x.RolId });
                    table.ForeignKey(
                        name: "FK_MenusRoles_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MenusRoles_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenusRoles_RolId",
                table: "MenusRoles",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenusRoles");

            migrationBuilder.AddColumn<int>(
                name: "RecursoId",
                table: "Menus",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_RecursoId",
                table: "Menus",
                column: "RecursoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Recursos_RecursoId",
                table: "Menus",
                column: "RecursoId",
                principalTable: "Recursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
