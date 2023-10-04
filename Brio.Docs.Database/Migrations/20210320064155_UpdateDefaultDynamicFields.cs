using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class UpdateDefaultDynamicFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "DynamicFieldInfos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExternalID = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    ObjectiveTypeID = table.Column<int>(type: "INTEGER", nullable: true),
                    ParentFieldID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicFieldInfos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DynamicFieldInfos_DynamicFieldInfos_ParentFieldID",
                        column: x => x.ParentFieldID,
                        principalTable: "DynamicFieldInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicFieldInfos_ObjectiveTypes_ObjectiveTypeID",
                        column: x => x.ObjectiveTypeID,
                        principalTable: "ObjectiveTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFieldInfos_ObjectiveTypeID",
                table: "DynamicFieldInfos",
                column: "ObjectiveTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFieldInfos_ParentFieldID",
                table: "DynamicFieldInfos",
                column: "ParentFieldID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DynamicFieldInfos");

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
    }
}
