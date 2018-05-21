using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NoeSbot.Database.Migrations
{
    public partial class AddCustomCommand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomCommandEntities",
                columns: table => new
                {
                    CustomCommandId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    GuildId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomCommandEntities", x => x.CustomCommandId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomCommandEntities");
        }
    }
}
