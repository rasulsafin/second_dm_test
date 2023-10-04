using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class AddObjectiveTypeIntoConnectionType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConnectionTypeID",
                table: "ObjectiveTypes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveTypes_ConnectionTypeID",
                table: "ObjectiveTypes",
                column: "ConnectionTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectiveTypes_ConnectionTypes_ConnectionTypeID",
                table: "ObjectiveTypes",
                column: "ConnectionTypeID",
                principalTable: "ConnectionTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectiveTypes_ConnectionTypes_ConnectionTypeID",
                table: "ObjectiveTypes");

            migrationBuilder.DropIndex(
                name: "IX_ObjectiveTypes_ConnectionTypeID",
                table: "ObjectiveTypes");

            migrationBuilder.DropColumn(
                name: "ConnectionTypeID",
                table: "ObjectiveTypes");
        }
    }
}
