using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botler.Migrations.Sqlite.Migrations
{
    public partial class AddMessageChannelIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DestinationChannelId",
                table: "Messages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginChannelId",
                table: "Messages",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationChannelId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "OriginChannelId",
                table: "Messages");
        }
    }
}
