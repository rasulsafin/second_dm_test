using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class AddConnectionInfoToDynamicFieldInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConnectionInfoID",
                table: "DynamicFields",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConnectionInfoID",
                table: "DynamicFieldInfos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFields_ConnectionInfoID",
                table: "DynamicFields",
                column: "ConnectionInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFieldInfos_ConnectionInfoID",
                table: "DynamicFieldInfos",
                column: "ConnectionInfoID");

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFieldInfos_ConnectionInfos_ConnectionInfoID",
                table: "DynamicFieldInfos",
                column: "ConnectionInfoID",
                principalTable: "ConnectionInfos",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFields_ConnectionInfos_ConnectionInfoID",
                table: "DynamicFields",
                column: "ConnectionInfoID",
                principalTable: "ConnectionInfos",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFieldInfos_ConnectionInfos_ConnectionInfoID",
                table: "DynamicFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFields_ConnectionInfos_ConnectionInfoID",
                table: "DynamicFields");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFields_ConnectionInfoID",
                table: "DynamicFields");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFieldInfos_ConnectionInfoID",
                table: "DynamicFieldInfos");

            migrationBuilder.DropColumn(
                name: "ConnectionInfoID",
                table: "DynamicFields");

            migrationBuilder.DropColumn(
                name: "ConnectionInfoID",
                table: "DynamicFieldInfos");
        }
    }
}
