using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarContabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "centros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_centros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_centros_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cuentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DescripcionSII = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Nif = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    EsNoOficial = table.Column<bool>(type: "bit", nullable: false),
                    EsObsoleta = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    CuentaPadreId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_cuentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cuentas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cuentas_cuentas_CuentaPadreId",
                        column: x => x.CuentaPadreId,
                        principalTable: "cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tiposdiario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    EsNoOficial = table.Column<bool>(type: "bit", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_tiposdiario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tiposdiario_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "diario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuentaId = table.Column<int>(type: "int", nullable: false),
                    TipoDiarioId = table.Column<int>(type: "int", nullable: true),
                    CentroCosteId = table.Column<int>(type: "int", nullable: true),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Asiento = table.Column<int>(type: "int", nullable: false),
                    NumLinea = table.Column<int>(type: "int", nullable: false),
                    NumDiario = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Debe = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Haber = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    EsDefinitivo = table.Column<bool>(type: "bit", nullable: false),
                    IdPunteo = table.Column<int>(type: "int", nullable: true),
                    FechaPunteo = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_diario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_diario_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_diario_centros_CentroCosteId",
                        column: x => x.CentroCosteId,
                        principalTable: "centros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_diario_cuentas_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_diario_tiposdiario_TipoDiarioId",
                        column: x => x.TipoDiarioId,
                        principalTable: "tiposdiario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "Recursos",
                columns: new[] { "Id", "Codigo" },
                values: new object[] { 14, "Contabilidad" });

            migrationBuilder.CreateIndex(
                name: "IX_centros_Codigo_EmpresaId",
                table: "centros",
                columns: new[] { "Codigo", "EmpresaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_centros_EmpresaId",
                table: "centros",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_Codigo_EmpresaId",
                table: "cuentas",
                columns: new[] { "Codigo", "EmpresaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_CuentaPadreId",
                table: "cuentas",
                column: "CuentaPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_EmpresaId",
                table: "cuentas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_diario_Asiento_NumLinea_EmpresaId",
                table: "diario",
                columns: new[] { "Asiento", "NumLinea", "EmpresaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_diario_CentroCosteId",
                table: "diario",
                column: "CentroCosteId");

            migrationBuilder.CreateIndex(
                name: "IX_diario_CuentaId_Fecha",
                table: "diario",
                columns: new[] { "CuentaId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_diario_EmpresaId",
                table: "diario",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_diario_Fecha",
                table: "diario",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_diario_TipoDiarioId",
                table: "diario",
                column: "TipoDiarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tiposdiario_Codigo_EmpresaId",
                table: "tiposdiario",
                columns: new[] { "Codigo", "EmpresaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tiposdiario_EmpresaId",
                table: "tiposdiario",
                column: "EmpresaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "diario");

            migrationBuilder.DropTable(
                name: "centros");

            migrationBuilder.DropTable(
                name: "cuentas");

            migrationBuilder.DropTable(
                name: "tiposdiario");

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 14);
        }
    }
}
