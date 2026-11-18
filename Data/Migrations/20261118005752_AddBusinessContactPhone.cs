using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceHubMVC.Data.Migrations
{
    public partial class AddBusinessContactPhone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Businesses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Businesses");
        }
    }
}
