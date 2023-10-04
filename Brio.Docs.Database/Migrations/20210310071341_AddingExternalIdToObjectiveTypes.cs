using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class AddingExternalIdToObjectiveTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "ObjectiveTypes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveTypes_ExternalId",
                table: "ObjectiveTypes",
                column: "ExternalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ObjectiveTypes_ExternalId",
                table: "ObjectiveTypes");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "ObjectiveTypes");
        }
    }
}
