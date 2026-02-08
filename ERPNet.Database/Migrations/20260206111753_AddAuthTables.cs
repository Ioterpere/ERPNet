using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntentosLogin",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DireccionIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    FechaIntento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Exitoso = table.Column<bool>(type: "bit", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentosLogin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntentosLogin_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaRevocacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReemplazadoPor = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntentosLogin_DireccionIp_FechaIntento",
                table: "IntentosLogin",
                columns: new[] { "DireccionIp", "FechaIntento" });

            migrationBuilder.CreateIndex(
                name: "IX_IntentosLogin_NombreUsuario_FechaIntento",
                table: "IntentosLogin",
                columns: new[] { "NombreUsuario", "FechaIntento" });

            migrationBuilder.CreateIndex(
                name: "IX_IntentosLogin_UsuarioId",
                table: "IntentosLogin",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioId",
                table: "RefreshTokens",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntentosLogin");

            migrationBuilder.DropTable(
                name: "RefreshTokens");
        }
    }
}
