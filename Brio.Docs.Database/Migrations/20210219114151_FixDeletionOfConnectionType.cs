using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class FixDeletionOfConnectionType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConnectionInfos_ConnectionTypes_ConnectionTypeID",
                table: "ConnectionInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_EnumerationTypes_ConnectionTypes_ConnectionTypeID",
                table: "EnumerationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ObjectiveTypes_ConnectionTypes_ConnectionTypeID",
                table: "ObjectiveTypes");

            migrationBuilder.AddForeignKey(
                name: "FK_ConnectionInfos_ConnectionTypes_ConnectionTypeID",
                table: "ConnectionInfos",
                column: "ConnectionTypeID",
                principalTable: "ConnectionTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EnumerationTypes_ConnectionTypes_ConnectionTypeID",
                table: "EnumerationTypes",
                column: "ConnectionTypeID",
                principalTable: "ConnectionTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectiveTypes_ConnectionTypes_ConnectionTypeID",
                table: "ObjectiveTypes",
                column: "ConnectionTypeID",
                principalTable: "ConnectionTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConnectionInfos_ConnectionTypes_ConnectionTypeID",
                table: "ConnectionInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_EnumerationTypes_ConnectionTypes_ConnectionTypeID",
                table: "EnumerationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ObjectiveTypes_ConnectionTypes_ConnectionTypeID",
                table: "ObjectiveTypes");

            migrationBuilder.AddForeignKey(
                name: "FK_ConnectionInfos_ConnectionTypes_ConnectionTypeID",
                table: "ConnectionInfos",
                column: "ConnectionTypeID",
                principalTable: "ConnectionTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EnumerationTypes_ConnectionTypes_ConnectionTypeID",
                table: "EnumerationTypes",
                column: "ConnectionTypeID",
                principalTable: "ConnectionTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectiveTypes_ConnectionTypes_ConnectionTypeID",
                table: "ObjectiveTypes",
                column: "ConnectionTypeID",
                principalTable: "ConnectionTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
