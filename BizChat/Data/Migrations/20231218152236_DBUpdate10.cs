using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BizChat.Data.Migrations
{
    public partial class DBUpdate10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerRoles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BanUserPerms = table.Column<bool>(type: "bit", nullable: false),
                    DeleteMessagePerms = table.Column<bool>(type: "bit", nullable: false),
                    KickUserPerms = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerRoles", x => x.Id);
                });
        }
    }
}
