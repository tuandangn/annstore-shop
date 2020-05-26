using Microsoft.EntityFrameworkCore.Migrations;

namespace Annstore.Web.Migrations
{
    public partial class AddCustomerFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Customer");

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Customer",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Customer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Customer");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
