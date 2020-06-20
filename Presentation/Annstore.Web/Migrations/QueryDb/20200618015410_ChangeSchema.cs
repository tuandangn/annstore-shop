using Microsoft.EntityFrameworkCore.Migrations;

namespace Annstore.Web.Migrations.QueryDb
{
    public partial class ChangeSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "query");

            migrationBuilder.RenameTable(
                name: "Category",
                schema: "qr",
                newName: "Category",
                newSchema: "query");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "qr");

            migrationBuilder.RenameTable(
                name: "Category",
                schema: "query",
                newName: "Category",
                newSchema: "qr");
        }
    }
}
