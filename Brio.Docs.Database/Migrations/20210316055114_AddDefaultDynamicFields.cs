using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class AddDefaultDynamicFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ObjectiveTypeID",
                table: "DynamicFields",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFields_ObjectiveTypeID",
                table: "DynamicFields",
                column: "ObjectiveTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFields_ObjectiveTypes_ObjectiveTypeID",
                table: "DynamicFields",
                column: "ObjectiveTypeID",
                principalTable: "ObjectiveTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFields_ObjectiveTypes_ObjectiveTypeID",
                table: "DynamicFields");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFields_ObjectiveTypeID",
                table: "DynamicFields");

            migrationBuilder.DropColumn(
                name: "ObjectiveTypeID",
                table: "DynamicFields");
        }
    }
}
