using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarArticulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuraciones_caducidad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DiasAviso = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuraciones_caducidad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "familias_articulo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    FamiliaPadreId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_familias_articulo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_familias_articulo_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_familias_articulo_familias_articulo_FamiliaPadreId",
                        column: x => x.FamiliaPadreId,
                        principalTable: "familias_articulo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "formatos_articulo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formatos_articulo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tipos_iva",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Porcentaje = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_iva", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "articulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    UnidadMedida = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PrecioCompra = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    PrecioVenta = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    FamiliaArticuloId = table.Column<int>(type: "integer", nullable: true),
                    TipoIvaId = table.Column<int>(type: "integer", nullable: true),
                    FormatoArticuloId = table.Column<int>(type: "integer", nullable: true),
                    ConfiguracionCaducidadId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<int>(type: "integer", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_articulos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_articulos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_articulos_configuraciones_caducidad_ConfiguracionCaducidadId",
                        column: x => x.ConfiguracionCaducidadId,
                        principalTable: "configuraciones_caducidad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_articulos_familias_articulo_FamiliaArticuloId",
                        column: x => x.FamiliaArticuloId,
                        principalTable: "familias_articulo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_articulos_formatos_articulo_FormatoArticuloId",
                        column: x => x.FormatoArticuloId,
                        principalTable: "formatos_articulo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_articulos_tipos_iva_TipoIvaId",
                        column: x => x.TipoIvaId,
                        principalTable: "tipos_iva",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "articulos_log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArticuloId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Nota = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StockAnterior = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    StockNuevo = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_articulos_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_articulos_log_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_articulos_log_articulos_ArticuloId",
                        column: x => x.ArticuloId,
                        principalTable: "articulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Recursos",
                columns: new[] { "Id", "Codigo" },
                values: new object[] { 13, "Articulos" });

            migrationBuilder.InsertData(
                table: "configuraciones_caducidad",
                columns: new[] { "Id", "DiasAviso", "Nombre" },
                values: new object[,]
                {
                    { 1, 7, "7 días antes" },
                    { 2, 15, "15 días antes" },
                    { 3, 30, "30 días antes" },
                    { 4, 60, "60 días antes" },
                    { 5, 90, "90 días antes" }
                });

            migrationBuilder.InsertData(
                table: "formatos_articulo",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Unidad" },
                    { 2, "Caja" },
                    { 3, "Palet" },
                    { 4, "Kilogramo" },
                    { 5, "Litro" }
                });

            migrationBuilder.InsertData(
                table: "tipos_iva",
                columns: new[] { "Id", "Nombre", "Porcentaje" },
                values: new object[,]
                {
                    { 1, "IVA 0%", 0m },
                    { 2, "IVA 4%", 4m },
                    { 3, "IVA 10%", 10m },
                    { 4, "IVA 21%", 21m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_articulos_Codigo_EmpresaId",
                table: "articulos",
                columns: new[] { "Codigo", "EmpresaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_articulos_ConfiguracionCaducidadId",
                table: "articulos",
                column: "ConfiguracionCaducidadId");

            migrationBuilder.CreateIndex(
                name: "IX_articulos_EmpresaId",
                table: "articulos",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_articulos_FamiliaArticuloId",
                table: "articulos",
                column: "FamiliaArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_articulos_FormatoArticuloId",
                table: "articulos",
                column: "FormatoArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_articulos_TipoIvaId",
                table: "articulos",
                column: "TipoIvaId");

            migrationBuilder.CreateIndex(
                name: "IX_articulos_log_ArticuloId",
                table: "articulos_log",
                column: "ArticuloId");

            migrationBuilder.CreateIndex(
                name: "IX_articulos_log_UsuarioId",
                table: "articulos_log",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_familias_articulo_EmpresaId",
                table: "familias_articulo",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_familias_articulo_FamiliaPadreId",
                table: "familias_articulo",
                column: "FamiliaPadreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "articulos_log");

            migrationBuilder.DropTable(
                name: "articulos");

            migrationBuilder.DropTable(
                name: "configuraciones_caducidad");

            migrationBuilder.DropTable(
                name: "familias_articulo");

            migrationBuilder.DropTable(
                name: "formatos_articulo");

            migrationBuilder.DropTable(
                name: "tipos_iva");

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 13);
        }
    }
}
