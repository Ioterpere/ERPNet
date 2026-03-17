using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class DropPolymorphicAddForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Archivos_Entidad_EntidadId",
                table: "Archivos");

            migrationBuilder.DropColumn(
                name: "Entidad",
                table: "Archivos");

            migrationBuilder.DropColumn(
                name: "EntidadId",
                table: "Archivos");

            migrationBuilder.DropColumn(
                name: "Etiqueta",
                table: "Archivos");

            migrationBuilder.AddColumn<Guid>(
                name: "CertificadoCeId",
                table: "Maquinarias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FichaTecnicaId",
                table: "Maquinarias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FotoId",
                table: "Maquinarias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManualId",
                table: "Maquinarias",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FotoId",
                table: "Empleados",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Maquinarias_CertificadoCeId",
                table: "Maquinarias",
                column: "CertificadoCeId",
                unique: true,
                filter: "[CertificadoCeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Maquinarias_FichaTecnicaId",
                table: "Maquinarias",
                column: "FichaTecnicaId",
                unique: true,
                filter: "[FichaTecnicaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Maquinarias_FotoId",
                table: "Maquinarias",
                column: "FotoId",
                unique: true,
                filter: "[FotoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Maquinarias_ManualId",
                table: "Maquinarias",
                column: "ManualId",
                unique: true,
                filter: "[ManualId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_FotoId",
                table: "Empleados",
                column: "FotoId",
                unique: true,
                filter: "[FotoId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Archivos_FotoId",
                table: "Empleados",
                column: "FotoId",
                principalTable: "Archivos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinarias_Archivos_CertificadoCeId",
                table: "Maquinarias",
                column: "CertificadoCeId",
                principalTable: "Archivos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinarias_Archivos_FichaTecnicaId",
                table: "Maquinarias",
                column: "FichaTecnicaId",
                principalTable: "Archivos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinarias_Archivos_FotoId",
                table: "Maquinarias",
                column: "FotoId",
                principalTable: "Archivos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinarias_Archivos_ManualId",
                table: "Maquinarias",
                column: "ManualId",
                principalTable: "Archivos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Archivos_FotoId",
                table: "Empleados");

            migrationBuilder.DropForeignKey(
                name: "FK_Maquinarias_Archivos_CertificadoCeId",
                table: "Maquinarias");

            migrationBuilder.DropForeignKey(
                name: "FK_Maquinarias_Archivos_FichaTecnicaId",
                table: "Maquinarias");

            migrationBuilder.DropForeignKey(
                name: "FK_Maquinarias_Archivos_FotoId",
                table: "Maquinarias");

            migrationBuilder.DropForeignKey(
                name: "FK_Maquinarias_Archivos_ManualId",
                table: "Maquinarias");

            migrationBuilder.DropIndex(
                name: "IX_Maquinarias_CertificadoCeId",
                table: "Maquinarias");

            migrationBuilder.DropIndex(
                name: "IX_Maquinarias_FichaTecnicaId",
                table: "Maquinarias");

            migrationBuilder.DropIndex(
                name: "IX_Maquinarias_FotoId",
                table: "Maquinarias");

            migrationBuilder.DropIndex(
                name: "IX_Maquinarias_ManualId",
                table: "Maquinarias");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_FotoId",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "CertificadoCeId",
                table: "Maquinarias");

            migrationBuilder.DropColumn(
                name: "FichaTecnicaId",
                table: "Maquinarias");

            migrationBuilder.DropColumn(
                name: "FotoId",
                table: "Maquinarias");

            migrationBuilder.DropColumn(
                name: "ManualId",
                table: "Maquinarias");

            migrationBuilder.DropColumn(
                name: "FotoId",
                table: "Empleados");

            migrationBuilder.AddColumn<string>(
                name: "Entidad",
                table: "Archivos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EntidadId",
                table: "Archivos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Etiqueta",
                table: "Archivos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Archivos_Entidad_EntidadId",
                table: "Archivos",
                columns: new[] { "Entidad", "EntidadId" });
        }
    }
}
