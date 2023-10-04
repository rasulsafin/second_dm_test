using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class UpdateBimElement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BimElements_Items_ItemID",
                table: "BimElements");

            migrationBuilder.DropIndex(
                name: "IX_BimElements_ItemID",
                table: "BimElements");

            migrationBuilder.DropColumn(
                name: "ItemID",
                table: "BimElements");

            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                table: "BimElements",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentName",
                table: "BimElements");

            migrationBuilder.AddColumn<int>(
                name: "ItemID",
                table: "BimElements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BimElements_ItemID",
                table: "BimElements",
                column: "ItemID");

            migrationBuilder.AddForeignKey(
                name: "FK_BimElements_Items_ItemID",
                table: "BimElements",
                column: "ItemID",
                principalTable: "Items",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
