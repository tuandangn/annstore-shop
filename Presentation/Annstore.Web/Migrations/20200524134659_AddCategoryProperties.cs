using Microsoft.EntityFrameworkCore.Migrations;

namespace AnnstoreShop.Web.Migrations
{
    public partial class AddCategoryProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Category",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Category",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Published",
                table: "Category",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "Published",
                table: "Category");
        }
    }
}
