﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BizChat.Data.Migrations
{
    public partial class DBUpdate6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Servers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Servers");
        }
    }
}
