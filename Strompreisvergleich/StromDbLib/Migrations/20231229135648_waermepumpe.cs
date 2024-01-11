using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StromDbLib.Migrations
{
    public partial class waermepumpe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWaermepumpe",
                table: "Stromverbrauch",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWaermepumpe",
                table: "Stromverbrauch");
        }
    }
}
