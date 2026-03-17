using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMultiempresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RolesUsuarios",
                table: "RolesUsuarios");

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Secciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "RolesUsuarios",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "RolesUsuarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Maquinas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaId",
                table: "Empleados",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolesUsuarios",
                table: "RolesUsuarios",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Cif = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioEmpresas",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    EsAdministrador = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioEmpresas", x => new { x.UsuarioId, x.EmpresaId });
                    table.ForeignKey(
                        name: "FK_UsuarioEmpresas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsuarioEmpresas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Recursos",
                columns: new[] { "Id", "Codigo" },
                values: new object[] { 11, "Empresas" });

            migrationBuilder.CreateIndex(
                name: "IX_Secciones_EmpresaId",
                table: "Secciones",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesUsuarios_EmpresaId",
                table: "RolesUsuarios",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_RolUsuario_UsuarioId_RolId_Empresa",
                table: "RolesUsuarios",
                columns: new[] { "UsuarioId", "RolId", "EmpresaId" },
                unique: true,
                filter: "[EmpresaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RolUsuario_UsuarioId_RolId_Global",
                table: "RolesUsuarios",
                columns: new[] { "UsuarioId", "RolId" },
                unique: true,
                filter: "[EmpresaId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Maquinas_EmpresaId",
                table: "Maquinas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_EmpresaId",
                table: "Empleados",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioEmpresas_EmpresaId",
                table: "UsuarioEmpresas",
                column: "EmpresaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Empresas_EmpresaId",
                table: "Empleados",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinas_Empresas_EmpresaId",
                table: "Maquinas",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RolesUsuarios_Empresas_EmpresaId",
                table: "RolesUsuarios",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Secciones_Empresas_EmpresaId",
                table: "Secciones",
                column: "EmpresaId",
                principalTable: "Empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Empresas_EmpresaId",
                table: "Empleados");

            migrationBuilder.DropForeignKey(
                name: "FK_Maquinas_Empresas_EmpresaId",
                table: "Maquinas");

            migrationBuilder.DropForeignKey(
                name: "FK_RolesUsuarios_Empresas_EmpresaId",
                table: "RolesUsuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Secciones_Empresas_EmpresaId",
                table: "Secciones");

            migrationBuilder.DropTable(
                name: "UsuarioEmpresas");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropIndex(
                name: "IX_Secciones_EmpresaId",
                table: "Secciones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RolesUsuarios",
                table: "RolesUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_RolesUsuarios_EmpresaId",
                table: "RolesUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_RolUsuario_UsuarioId_RolId_Empresa",
                table: "RolesUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_RolUsuario_UsuarioId_RolId_Global",
                table: "RolesUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_Maquinas_EmpresaId",
                table: "Maquinas");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_EmpresaId",
                table: "Empleados");

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Secciones");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "RolesUsuarios");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "RolesUsuarios");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Maquinas");

            migrationBuilder.DropColumn(
                name: "EmpresaId",
                table: "Empleados");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RolesUsuarios",
                table: "RolesUsuarios",
                columns: new[] { "UsuarioId", "RolId" });
        }
    }
}
