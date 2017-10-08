using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NoeSbot.Database.Migrations
{
    public partial class NotifyItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotifyItemEntities",
                columns: table => new
                {
                    NotifyItemId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    GuildId = table.Column<long>(nullable: false),
                    Logo = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotifyItemEntities", x => x.NotifyItemId);
                });

            migrationBuilder.CreateTable(
                name: "NotifyRole",
                columns: table => new
                {
                    NotifyRoleId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    NotifyItemId = table.Column<int>(nullable: true),
                    RoleId = table.Column<long>(nullable: false),
                    Rolename = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotifyRole", x => x.NotifyRoleId);
                    table.ForeignKey(
                        name: "FK_NotifyRole_NotifyItemEntities_NotifyItemId",
                        column: x => x.NotifyItemId,
                        principalTable: "NotifyItemEntities",
                        principalColumn: "NotifyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotifyUser",
                columns: table => new
                {
                    NotifyUserId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    NotifyItemId = table.Column<int>(nullable: true),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotifyUser", x => x.NotifyUserId);
                    table.ForeignKey(
                        name: "FK_NotifyUser_NotifyItemEntities_NotifyItemId",
                        column: x => x.NotifyItemId,
                        principalTable: "NotifyItemEntities",
                        principalColumn: "NotifyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotifyRole_NotifyItemId",
                table: "NotifyRole",
                column: "NotifyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_NotifyUser_NotifyItemId",
                table: "NotifyUser",
                column: "NotifyItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotifyRole");

            migrationBuilder.DropTable(
                name: "NotifyUser");

            migrationBuilder.DropTable(
                name: "NotifyItemEntities");
        }
    }
}
