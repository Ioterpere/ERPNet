using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Maquinas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropForeignKey(
                name: "FK_Maquinarias_Secciones_SeccionId",
                table: "Maquinarias");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesMantenimiento_Maquinarias_MaquinariaId",
                table: "OrdenesMantenimiento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Maquinarias",
                table: "Maquinarias");

            migrationBuilder.RenameTable(
                name: "Maquinarias",
                newName: "Maquinas");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinarias_SeccionId",
                table: "Maquinas",
                newName: "IX_Maquinas_SeccionId");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinarias_ManualId",
                table: "Maquinas",
                newName: "IX_Maquinas_ManualId");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinarias_FotoId",
                table: "Maquinas",
                newName: "IX_Maquinas_FotoId");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinarias_FichaTecnicaId",
                table: "Maquinas",
                newName: "IX_Maquinas_FichaTecnicaId");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinarias_Codigo",
                table: "Maquinas",
                newName: "IX_Maquinas_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinarias_CertificadoCeId",
                table: "Maquinas",
                newName: "IX_Maquinas_CertificadoCeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Maquinas",
                table: "Maquinas",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinas_Archivos_CertificadoCeId",
                table: "Maquinas",
                column: "CertificadoCeId",
                principalTable: "Archivos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinas_Archivos_FichaTecnicaId",
                table: "Maquinas",
                column: "FichaTecnicaId",
                principalTable: "Archivos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinas_Archivos_FotoId",
                table: "Maquinas",
                column: "FotoId",
                principalTable: "Archivos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinas_Archivos_ManualId",
                table: "Maquinas",
                column: "ManualId",
                principalTable: "Archivos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinas_Secciones_SeccionId",
                table: "Maquinas",
                column: "SeccionId",
                principalTable: "Secciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesMantenimiento_Maquinas_MaquinariaId",
                table: "OrdenesMantenimiento",
                column: "MaquinariaId",
                principalTable: "Maquinas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Maquinas_Archivos_CertificadoCeId",
                table: "Maquinas");

            migrationBuilder.DropForeignKey(
                name: "FK_Maquinas_Archivos_FichaTecnicaId",
                table: "Maquinas");

            migrationBuilder.DropForeignKey(
                name: "FK_Maquinas_Archivos_FotoId",
                table: "Maquinas");

            migrationBuilder.DropForeignKey(
                name: "FK_Maquinas_Archivos_ManualId",
                table: "Maquinas");

            migrationBuilder.DropForeignKey(
                name: "FK_Maquinas_Secciones_SeccionId",
                table: "Maquinas");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdenesMantenimiento_Maquinas_MaquinariaId",
                table: "OrdenesMantenimiento");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Maquinas",
                table: "Maquinas");

            migrationBuilder.RenameTable(
                name: "Maquinas",
                newName: "Maquinarias");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinas_SeccionId",
                table: "Maquinarias",
                newName: "IX_Maquinarias_SeccionId");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinas_ManualId",
                table: "Maquinarias",
                newName: "IX_Maquinarias_ManualId");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinas_FotoId",
                table: "Maquinarias",
                newName: "IX_Maquinarias_FotoId");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinas_FichaTecnicaId",
                table: "Maquinarias",
                newName: "IX_Maquinarias_FichaTecnicaId");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinas_Codigo",
                table: "Maquinarias",
                newName: "IX_Maquinarias_Codigo");

            migrationBuilder.RenameIndex(
                name: "IX_Maquinas_CertificadoCeId",
                table: "Maquinarias",
                newName: "IX_Maquinarias_CertificadoCeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Maquinarias",
                table: "Maquinarias",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Maquinarias_Secciones_SeccionId",
                table: "Maquinarias",
                column: "SeccionId",
                principalTable: "Secciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdenesMantenimiento_Maquinarias_MaquinariaId",
                table: "OrdenesMantenimiento",
                column: "MaquinariaId",
                principalTable: "Maquinarias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
