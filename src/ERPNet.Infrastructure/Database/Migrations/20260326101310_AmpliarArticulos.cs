using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarArticulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrecioCompra",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "articulos");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioVenta",
                table: "articulos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoBarras",
                table: "articulos",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsPropio",
                table: "articulos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Depreciacion",
                table: "articulos",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DescripcionVenta",
                table: "articulos",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiasVidaUtil",
                table: "articulos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsInventariable",
                table: "articulos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsNuevo",
                table: "articulos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsObsoleto",
                table: "articulos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FilasPalet",
                table: "articulos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LeadTime",
                table: "articulos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NivelPedido",
                table: "articulos",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NivelReposicion",
                table: "articulos",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "articulos",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PesoGramo",
                table: "articulos",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCoste",
                table: "articulos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioMedio",
                table: "articulos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ProveedorPrincipal",
                table: "articulos",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StockMinimo",
                table: "articulos",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UnidadMedida2",
                table: "articulos",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnidadesCaja",
                table: "articulos",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "UnidadesPalet",
                table: "articulos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoBarras",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "Depreciacion",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "DescripcionVenta",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "DiasVidaUtil",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "EsInventariable",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "EsNuevo",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "EsObsoleto",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "FilasPalet",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "LeadTime",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "NivelPedido",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "NivelReposicion",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "PesoGramo",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "PrecioCoste",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "PrecioMedio",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "ProveedorPrincipal",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "StockMinimo",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "UnidadMedida2",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "UnidadesCaja",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "UnidadesPalet",
                table: "articulos");

            migrationBuilder.DropColumn(
                name: "EsPropio",
                table: "articulos");

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "articulos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioVenta",
                table: "articulos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCompra",
                table: "articulos",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);
        }
    }
}
