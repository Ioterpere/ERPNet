using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRecursoAsistenteIa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Recursos",
                columns: new[] { "Id", "Codigo" },
                values: new object[] { 12, "AsistenteIa" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Recursos",
                keyColumn: "Id",
                keyValue: 12);
        }
    }
}
