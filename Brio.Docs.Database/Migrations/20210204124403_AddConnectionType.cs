using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class AddConnectionType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_ConnectionInfoID",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "ConnectionTypeID",
                table: "ConnectionInfos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ConnectionTypes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionTypes", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ConnectionInfoID",
                table: "Users",
                column: "ConnectionInfoID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionInfos_ConnectionTypeID",
                table: "ConnectionInfos",
                column: "ConnectionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionTypes_Name",
                table: "ConnectionTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConnectionInfos_ConnectionTypes_ConnectionTypeID",
                table: "ConnectionInfos",
                column: "ConnectionTypeID",
                principalTable: "ConnectionTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConnectionInfos_ConnectionTypes_ConnectionTypeID",
                table: "ConnectionInfos");

            migrationBuilder.DropTable(
                name: "ConnectionTypes");

            migrationBuilder.DropIndex(
                name: "IX_Users_ConnectionInfoID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_ConnectionInfos_ConnectionTypeID",
                table: "ConnectionInfos");

            migrationBuilder.DropColumn(
                name: "ConnectionTypeID",
                table: "ConnectionInfos");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ConnectionInfoID",
                table: "Users",
                column: "ConnectionInfoID");
        }
    }
}
