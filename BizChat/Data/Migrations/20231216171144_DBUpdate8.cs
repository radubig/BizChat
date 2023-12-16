using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BizChat.Data.Migrations
{
    public partial class DBUpdate8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Servers_ServerId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Categories_CategoryId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Channels_ChannelId",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Servers_ServerId",
                table: "Categories",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Categories_CategoryId",
                table: "Channels",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Channels_ChannelId",
                table: "Messages",
                column: "ChannelId",
                principalTable: "Channels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Servers_ServerId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Categories_CategoryId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Channels_ChannelId",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Servers_ServerId",
                table: "Categories",
                column: "ServerId",
                principalTable: "Servers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Categories_CategoryId",
                table: "Channels",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Channels_ChannelId",
                table: "Messages",
                column: "ChannelId",
                principalTable: "Channels",
                principalColumn: "Id");
        }
    }
}
