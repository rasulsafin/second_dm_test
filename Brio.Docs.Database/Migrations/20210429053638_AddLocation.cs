using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class AddLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationID",
                table: "Objectives",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PositionX = table.Column<float>(type: "REAL", nullable: false),
                    PositionY = table.Column<float>(type: "REAL", nullable: false),
                    PositionZ = table.Column<float>(type: "REAL", nullable: false),
                    CameraPositionX = table.Column<float>(type: "REAL", nullable: false),
                    CameraPositionY = table.Column<float>(type: "REAL", nullable: false),
                    CameraPositionZ = table.Column<float>(type: "REAL", nullable: false),
                    BimElementID = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.ID);
                });

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objectives_Location_LocationID",
                table: "Objectives");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_LocationID",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "LocationID",
                table: "Objectives");
        }
    }
}
