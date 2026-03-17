using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CambiosenMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IconClass",
                table: "Menus",
                newName: "Tag");

            migrationBuilder.RenameColumn(
                name: "CustomClass",
                table: "Menus",
                newName: "Icon");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Tag",
                table: "Menus",
                newName: "IconClass");

            migrationBuilder.RenameColumn(
                name: "Icon",
                table: "Menus",
                newName: "CustomClass");
        }
    }
}
