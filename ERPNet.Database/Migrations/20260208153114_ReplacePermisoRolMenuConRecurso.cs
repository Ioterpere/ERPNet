using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Database.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePermisoRolMenuConRecurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermisosRolMenu");

            migrationBuilder.AddColumn<int>(
                name: "RecursoId",
                table: "Menus",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Recursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recursos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PermisosRolRecurso",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "int", nullable: false),
                    RecursoId = table.Column<int>(type: "int", nullable: false),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    Alcance = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermisosRolRecurso", x => new { x.RolId, x.RecursoId });
                    table.ForeignKey(
                        name: "FK_PermisosRolRecurso_Recursos_RecursoId",
                        column: x => x.RecursoId,
                        principalTable: "Recursos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermisosRolRecurso_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Menus_RecursoId",
                table: "Menus",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_PermisosRolRecurso_RecursoId",
                table: "PermisosRolRecurso",
                column: "RecursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_Codigo",
                table: "Recursos",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Recursos_RecursoId",
                table: "Menus",
                column: "RecursoId",
                principalTable: "Recursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Recursos_RecursoId",
                table: "Menus");

            migrationBuilder.DropTable(
                name: "PermisosRolRecurso");

            migrationBuilder.DropTable(
                name: "Recursos");

            migrationBuilder.DropIndex(
                name: "IX_Menus_RecursoId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "RecursoId",
                table: "Menus");

            migrationBuilder.CreateTable(
                name: "PermisosRolMenu",
                columns: table => new
                {
                    RolId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    Alcance = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CanCreate = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CanEdit = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermisosRolMenu", x => new { x.RolId, x.MenuId });
                    table.ForeignKey(
                        name: "FK_PermisosRolMenu_Menus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermisosRolMenu_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermisosRolMenu_MenuId",
                table: "PermisosRolMenu",
                column: "MenuId");
        }
    }
}
