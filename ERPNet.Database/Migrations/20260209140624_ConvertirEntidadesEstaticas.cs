using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERPNet.Database.Migrations
{
    /// <inheritdoc />
    public partial class ConvertirEntidadesEstaticas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recursos_Codigo",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TiposMantenimiento");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TiposMantenimiento");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TiposMantenimiento");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "TiposMantenimiento");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TiposMantenimiento");

            migrationBuilder.DropColumn(
                name: "Nombre",
                table: "TiposMantenimiento");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TiposMantenimiento");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TiposMantenimiento");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Recursos");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Recursos");

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "TiposMantenimiento",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Recursos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.InsertData(
                table: "Recursos",
                columns: new[] { "Id", "Codigo" },
                values: new object[,]
                {
                    { 1, "Aplicacion" },
                    { 2, "Empleados" },
                    { 3, "Vacaciones" },
                    { 4, "Turnos" },
                    { 5, "Maquinaria" },
                    { 6, "Mantenimiento" }
                });

            migrationBuilder.InsertData(
                table: "TiposMantenimiento",
                columns: new[] { "Id", "Codigo" },
                values: new object[,]
                {
                    { 1, "Correctivo" },
                    { 2, "Preventivo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TiposMantenimiento",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "TiposMantenimiento");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TiposMantenimiento",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "TiposMantenimiento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TiposMantenimiento",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "TiposMantenimiento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TiposMantenimiento",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "TiposMantenimiento",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TiposMantenimiento",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "TiposMantenimiento",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Recursos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Recursos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Recursos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Recursos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "Recursos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Recursos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Recursos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Recursos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Recursos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recursos_Codigo",
                table: "Recursos",
                column: "Codigo",
                unique: true);
        }
    }
}
