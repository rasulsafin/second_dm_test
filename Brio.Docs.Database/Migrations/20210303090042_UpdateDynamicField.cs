using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class UpdateDynamicField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Key",
                table: "DynamicFields",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "ObjectiveID",
                table: "DynamicFields",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "DynamicFields",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentFieldID",
                table: "DynamicFields",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFields_ParentFieldID",
                table: "DynamicFields",
                column: "ParentFieldID");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFields_DynamicFields_ParentFieldID",
                table: "DynamicFields",
                column: "ParentFieldID",
                principalTable: "DynamicFields",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFields_DynamicFields_ParentFieldID",
                table: "DynamicFields");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFields_ParentFieldID",
                table: "DynamicFields");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "DynamicFields");

            migrationBuilder.DropColumn(
                name: "ParentFieldID",
                table: "DynamicFields");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "DynamicFields",
                newName: "Key");

            migrationBuilder.AlterColumn<int>(
                name: "ObjectiveID",
                table: "DynamicFields",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
