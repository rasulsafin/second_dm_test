using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class AddItemToLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objectives_Location_LocationID",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_LocationID",
                table: "Objectives");

            migrationBuilder.RenameColumn(
                name: "BimElementID",
                table: "Location",
                newName: "Guid");

            migrationBuilder.AddColumn<int>(
                name: "ItemID",
                table: "Location",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ObjectiveID",
                table: "Location",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Location_ItemID",
                table: "Location",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ObjectiveID",
                table: "Location",
                column: "ObjectiveID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Location_Items_ItemID",
                table: "Location",
                column: "ItemID",
                principalTable: "Items",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Location_Objectives_ObjectiveID",
                table: "Location",
                column: "ObjectiveID",
                principalTable: "Objectives",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Location_Items_ItemID",
                table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_Location_Objectives_ObjectiveID",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_ItemID",
                table: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Location_ObjectiveID",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "ItemID",
                table: "Location");

            migrationBuilder.DropColumn(
                name: "ObjectiveID",
                table: "Location");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "Location",
                newName: "BimElementID");

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_LocationID",
                table: "Objectives",
                column: "LocationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Objectives_Location_LocationID",
                table: "Objectives",
                column: "LocationID",
                principalTable: "Location",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
