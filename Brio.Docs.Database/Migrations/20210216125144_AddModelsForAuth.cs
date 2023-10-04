using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class AddModelsForAuth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppProperty",
                table: "ConnectionTypes");

            migrationBuilder.DropColumn(
                name: "AuthFieldNames",
                table: "ConnectionTypes");

            migrationBuilder.DropColumn(
                name: "AuthFieldValues",
                table: "ConnectionInfos");

            migrationBuilder.CreateTable(
                name: "AppProperties",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    ConnectionTypeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProperties", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AppProperties_ConnectionTypes_ConnectionTypeID",
                        column: x => x.ConnectionTypeID,
                        principalTable: "ConnectionTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthFieldNames",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    ConnectionTypeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthFieldNames", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AuthFieldNames_ConnectionTypes_ConnectionTypeID",
                        column: x => x.ConnectionTypeID,
                        principalTable: "ConnectionTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthFieldValues",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    ConnectionInfoID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthFieldValues", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AuthFieldValues_ConnectionInfos_ConnectionInfoID",
                        column: x => x.ConnectionInfoID,
                        principalTable: "ConnectionInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppProperties_ConnectionTypeID",
                table: "AppProperties",
                column: "ConnectionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthFieldNames_ConnectionTypeID",
                table: "AuthFieldNames",
                column: "ConnectionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthFieldValues_ConnectionInfoID",
                table: "AuthFieldValues",
                column: "ConnectionInfoID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppProperties");

            migrationBuilder.DropTable(
                name: "AuthFieldNames");

            migrationBuilder.DropTable(
                name: "AuthFieldValues");

            migrationBuilder.AddColumn<string>(
                name: "AppProperty",
                table: "ConnectionTypes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthFieldNames",
                table: "ConnectionTypes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthFieldValues",
                table: "ConnectionInfos",
                type: "TEXT",
                nullable: true);
        }
    }
}
