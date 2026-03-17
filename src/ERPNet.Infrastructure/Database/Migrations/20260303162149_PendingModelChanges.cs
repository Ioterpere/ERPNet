using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPNet.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase(
                collation: "Latin1_General_CI_AI");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase(
                oldCollation: "Latin1_General_CI_AI");
        }
    }
}
