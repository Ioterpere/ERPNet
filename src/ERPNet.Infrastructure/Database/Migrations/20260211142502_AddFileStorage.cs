using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFileStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Archivos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NombreOriginal = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tamaño = table.Column<long>(type: "bigint", nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntidadId = table.Column<int>(type: "int", nullable: false),
                    Etiqueta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EsThumbnail = table.Column<bool>(type: "bit", nullable: false),
                    ArchivoOriginalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<int>(type: "int", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Archivos_Archivos_ArchivoOriginalId",
                        column: x => x.ArchivoOriginalId,
                        principalTable: "Archivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Archivos_ArchivoOriginalId",
                table: "Archivos",
                column: "ArchivoOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_Archivos_Entidad_EntidadId",
                table: "Archivos",
                columns: new[] { "Entidad", "EntidadId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Archivos");
        }
    }
}
