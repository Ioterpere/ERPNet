using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarParametrosCuenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClienteAsociadoId",
                table: "cuentas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConceptoAnaliticaId",
                table: "cuentas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuentaAmortizacionId",
                table: "cuentas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuentaPagoDelegadoId",
                table: "cuentas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmpresaVinculadaId",
                table: "cuentas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_CuentaAmortizacionId",
                table: "cuentas",
                column: "CuentaAmortizacionId");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_CuentaPagoDelegadoId",
                table: "cuentas",
                column: "CuentaPagoDelegadoId");

            migrationBuilder.CreateIndex(
                name: "IX_cuentas_EmpresaVinculadaId",
                table: "cuentas",
                column: "EmpresaVinculadaId");

            migrationBuilder.AddForeignKey(
                name: "FK_cuentas_Empresas_EmpresaVinculadaId",
                table: "cuentas",
                column: "EmpresaVinculadaId",
                principalTable: "Empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_cuentas_cuentas_CuentaAmortizacionId",
                table: "cuentas",
                column: "CuentaAmortizacionId",
                principalTable: "cuentas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_cuentas_cuentas_CuentaPagoDelegadoId",
                table: "cuentas",
                column: "CuentaPagoDelegadoId",
                principalTable: "cuentas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cuentas_Empresas_EmpresaVinculadaId",
                table: "cuentas");

            migrationBuilder.DropForeignKey(
                name: "FK_cuentas_cuentas_CuentaAmortizacionId",
                table: "cuentas");

            migrationBuilder.DropForeignKey(
                name: "FK_cuentas_cuentas_CuentaPagoDelegadoId",
                table: "cuentas");

            migrationBuilder.DropIndex(
                name: "IX_cuentas_CuentaAmortizacionId",
                table: "cuentas");

            migrationBuilder.DropIndex(
                name: "IX_cuentas_CuentaPagoDelegadoId",
                table: "cuentas");

            migrationBuilder.DropIndex(
                name: "IX_cuentas_EmpresaVinculadaId",
                table: "cuentas");

            migrationBuilder.DropColumn(
                name: "ClienteAsociadoId",
                table: "cuentas");

            migrationBuilder.DropColumn(
                name: "ConceptoAnaliticaId",
                table: "cuentas");

            migrationBuilder.DropColumn(
                name: "CuentaAmortizacionId",
                table: "cuentas");

            migrationBuilder.DropColumn(
                name: "CuentaPagoDelegadoId",
                table: "cuentas");

            migrationBuilder.DropColumn(
                name: "EmpresaVinculadaId",
                table: "cuentas");
        }
    }
}
