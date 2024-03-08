using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botler.Database.Migrations
{
    public partial class InitialMssql : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    InternalId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginPlatform = table.Column<int>(type: "int", nullable: false),
                    OriginMessageId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OriginChannelId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginAuthor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageBody = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageAdditionalDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestinationPlatform = table.Column<int>(type: "int", nullable: false),
                    DestinationMessageId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DestinationChannelId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false)
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
