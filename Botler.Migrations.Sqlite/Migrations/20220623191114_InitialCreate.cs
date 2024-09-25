using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botler.Migrations.Sqlite.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    InternalId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OriginPlatform = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginMessageId = table.Column<string>(type: "TEXT", nullable: false),
                    OriginAuthor = table.Column<string>(type: "TEXT", nullable: false),
                    MessageBody = table.Column<string>(type: "TEXT", nullable: false),
                    MessageAdditionalDataJson = table.Column<string>(type: "TEXT", nullable: true),
                    DestinationPlatform = table.Column<int>(type: "INTEGER", nullable: false),
                    DestinationMessageId = table.Column<string>(type: "TEXT", nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsEdited = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.InternalId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_DestinationMessageId_DestinationPlatform",
                table: "Messages",
                columns: new[] { "DestinationMessageId", "DestinationPlatform" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_OriginMessageId_OriginPlatform",
                table: "Messages",
                columns: new[] { "OriginMessageId", "OriginPlatform" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
