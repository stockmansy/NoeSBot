using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NoeSbot.Database.Migrations
{
    public partial class AddActivityLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogEntities",
                columns: table => new
                {
                    ActivityLogId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    GuildId = table.Column<long>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogEntities", x => x.ActivityLogId);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogItem",
                columns: table => new
                {
                    ActivityLogItemId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    ActivityLogId = table.Column<int>(nullable: true),
                    ChannelId = table.Column<long>(nullable: false),
                    Command = table.Column<string>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    Log = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogItem", x => x.ActivityLogItemId);
                    table.ForeignKey(
                        name: "FK_ActivityLogItem_ActivityLogEntities_ActivityLogId",
                        column: x => x.ActivityLogId,
                        principalTable: "ActivityLogEntities",
                        principalColumn: "ActivityLogId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogItem_ActivityLogId",
                table: "ActivityLogItem",
                column: "ActivityLogId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogItem");

            migrationBuilder.DropTable(
                name: "ActivityLogEntities");
        }
    }
}
