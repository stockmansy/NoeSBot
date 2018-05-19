using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NoeSbot.Database.Migrations
{
    public partial class AddProfileBackground : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileBackgroundEntities",
                columns: table => new
                {
                    ProfileBackgroundId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    GuildId = table.Column<long>(nullable: false),
                    ProfileBackgroundSettingId = table.Column<int>(nullable: false),
                    UserId = table.Column<long>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileBackgroundEntities", x => x.ProfileBackgroundId);
                });

            migrationBuilder.CreateTable(
                name: "ProfileBackgroundAlias",
                columns: table => new
                {
                    AliasId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    Alias = table.Column<string>(nullable: true),
                    ProfileBackgroundId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileBackgroundAlias", x => x.AliasId);
                    table.ForeignKey(
                        name: "FK_ProfileBackgroundAlias_ProfileBackgroundEntities_ProfileBackgroundId",
                        column: x => x.ProfileBackgroundId,
                        principalTable: "ProfileBackgroundEntities",
                        principalColumn: "ProfileBackgroundId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileBackgroundAlias_ProfileBackgroundId",
                table: "ProfileBackgroundAlias",
                column: "ProfileBackgroundId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileBackgroundAlias");

            migrationBuilder.DropTable(
                name: "ProfileBackgroundEntities");
        }
    }
}
