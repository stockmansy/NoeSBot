using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NoeSbot.Migrations
{
    public partial class Punished : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PunishedEntities",
                columns: table => new
                {
                    PunishedId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    Duration = table.Column<int>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    TimeOfPunishment = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PunishedEntities", x => x.PunishedId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PunishedEntities");
        }
    }
}
