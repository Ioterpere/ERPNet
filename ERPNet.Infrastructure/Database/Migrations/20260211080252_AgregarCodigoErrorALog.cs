using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCodigoErrorALog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoError",
                table: "Logs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoError",
                table: "Logs");
        }
    }
}
