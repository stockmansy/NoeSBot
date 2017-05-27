using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NoeSbot.Database.Migrations
{
    public partial class Profile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileEntities",
                columns: table => new
                {
                    ProfileId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    GuildId = table.Column<long>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileEntities", x => x.ProfileId);
                });

            migrationBuilder.CreateTable(
                name: "ProfileItemEntities",
                columns: table => new
                {
                    ProfileItemId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    ProfileId = table.Column<int>(nullable: false),
                    ProfileItemTypeId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileItemEntities", x => x.ProfileItemId);
                    table.ForeignKey(
                        name: "FK_ProfileItemEntities_ProfileEntities_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "ProfileEntities",
                        principalColumn: "ProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileItemEntities_ProfileId",
                table: "ProfileItemEntities",
                column: "ProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileItemEntities");

            migrationBuilder.DropTable(
                name: "ProfileEntities");
        }
    }
}
