using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StromDbLib.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Strompreis",
                columns: table => new
                {
                    StrompreisId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Von = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Bis = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Preis = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strompreis", x => x.StrompreisId);
                });

            migrationBuilder.CreateTable(
                name: "Stromverbrauch",
                columns: table => new
                {
                    StromverbrauchId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Zeitpunkt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Verbrauch = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stromverbrauch", x => x.StromverbrauchId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Strompreis");

            migrationBuilder.DropTable(
                name: "Stromverbrauch");
        }
    }
}
