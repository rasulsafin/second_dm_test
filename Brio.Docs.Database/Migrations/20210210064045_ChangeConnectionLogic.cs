using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class ChangeConnectionLogic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthFieldNames",
                table: "ConnectionInfos");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ConnectionInfos",
                newName: "AuthFieldValues");

            migrationBuilder.AddColumn<string>(
                name: "AppProperty",
                table: "ConnectionTypes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthFieldNames",
                table: "ConnectionTypes",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppProperty",
                table: "ConnectionTypes");

            migrationBuilder.DropColumn(
                name: "AuthFieldNames",
                table: "ConnectionTypes");

            migrationBuilder.RenameColumn(
                name: "AuthFieldValues",
                table: "ConnectionInfos",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "AuthFieldNames",
                table: "ConnectionInfos",
                type: "TEXT",
                nullable: true);
        }
    }
}
