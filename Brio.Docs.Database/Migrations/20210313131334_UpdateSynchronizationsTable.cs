using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class UpdateSynchronizationsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Synchronizations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Synchronizations_UserID",
                table: "Synchronizations",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Synchronizations_Users_UserID",
                table: "Synchronizations",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Synchronizations_Users_UserID",
                table: "Synchronizations");

            migrationBuilder.DropIndex(
                name: "IX_Synchronizations_UserID",
                table: "Synchronizations");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Synchronizations");
        }
    }
}
