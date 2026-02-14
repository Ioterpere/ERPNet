using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Nuevosrecursos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 5,
                column: "Codigo",
                value: "Marcajes");

            migrationBuilder.UpdateData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 6,
                column: "Codigo",
                value: "Maquinaria");

            migrationBuilder.InsertData(
                table: "Recursos",
                columns: new[] { "Id", "Codigo" },
                values: new object[,]
                {
                    { 7, "Mantenimiento" },
                    { 8, "OrdenesFabrica" },
                    { 9, "Clientes" },
                    { 10, "Facturas" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 5,
                column: "Codigo",
                value: "Maquinaria");

            migrationBuilder.UpdateData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 6,
                column: "Codigo",
                value: "Mantenimiento");
        }
    }
}
