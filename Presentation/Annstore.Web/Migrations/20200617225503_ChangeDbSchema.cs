using Microsoft.EntityFrameworkCore.Migrations;

namespace Annstore.Web.Migrations
{
    public partial class ChangeDbSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dm");

            migrationBuilder.RenameTable(
                name: "Customer",
                newName: "Customer",
                newSchema: "dm");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Category",
                newSchema: "dm");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Customer",
                schema: "dm",
                newName: "Customer");

            migrationBuilder.RenameTable(
                name: "Category",
                schema: "dm",
                newName: "Category");
        }
    }
}
