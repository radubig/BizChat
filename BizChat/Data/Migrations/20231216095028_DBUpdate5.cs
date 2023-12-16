using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BizChat.Data.Migrations
{
    public partial class DBUpdate5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsModerator",
                table: "ServerUsers",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsModerator",
                table: "ServerUsers");
        }
    }
}
