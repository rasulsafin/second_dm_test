using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class UpdateEnumerationType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserEnumDmValues");

            migrationBuilder.DropTable(
                name: "EnumDmValues");

            migrationBuilder.DropTable(
                name: "EnumDms");

            migrationBuilder.CreateTable(
                name: "EnumerationTypes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    ConnectionTypeID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumerationTypes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EnumerationTypes_ConnectionTypes_ConnectionTypeID",
                        column: x => x.ConnectionTypeID,
                        principalTable: "ConnectionTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConnectionInfoEnumerationTypes",
                columns: table => new
                {
                    ConnectionInfoID = table.Column<int>(type: "INTEGER", nullable: false),
                    EnumerationTypeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionInfoEnumerationTypes", x => new { x.ConnectionInfoID, x.EnumerationTypeID });
                    table.ForeignKey(
                        name: "FK_ConnectionInfoEnumerationTypes_ConnectionInfos_ConnectionInfoID",
                        column: x => x.ConnectionInfoID,
                        principalTable: "ConnectionInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConnectionInfoEnumerationTypes_EnumerationTypes_EnumerationTypeID",
                        column: x => x.EnumerationTypeID,
                        principalTable: "EnumerationTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnumerationValues",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    EnumerationTypeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumerationValues", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EnumerationValues_EnumerationTypes_EnumerationTypeID",
                        column: x => x.EnumerationTypeID,
                        principalTable: "EnumerationTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectionInfoEnumerationValues",
                columns: table => new
                {
                    ConnectionInfoID = table.Column<int>(type: "INTEGER", nullable: false),
                    EnumerationValueID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionInfoEnumerationValues", x => new { x.ConnectionInfoID, x.EnumerationValueID });
                    table.ForeignKey(
                        name: "FK_ConnectionInfoEnumerationValues_ConnectionInfos_ConnectionInfoID",
                        column: x => x.ConnectionInfoID,
                        principalTable: "ConnectionInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConnectionInfoEnumerationValues_EnumerationValues_EnumerationValueID",
                        column: x => x.EnumerationValueID,
                        principalTable: "EnumerationValues",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionInfoEnumerationTypes_EnumerationTypeID",
                table: "ConnectionInfoEnumerationTypes",
                column: "EnumerationTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionInfoEnumerationValues_EnumerationValueID",
                table: "ConnectionInfoEnumerationValues",
                column: "EnumerationValueID");

            migrationBuilder.CreateIndex(
                name: "IX_EnumerationTypes_ConnectionTypeID",
                table: "EnumerationTypes",
                column: "ConnectionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_EnumerationValues_EnumerationTypeID",
                table: "EnumerationValues",
                column: "EnumerationTypeID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectionInfoEnumerationTypes");

            migrationBuilder.DropTable(
                name: "ConnectionInfoEnumerationValues");

            migrationBuilder.DropTable(
                name: "EnumerationValues");

            migrationBuilder.DropTable(
                name: "EnumerationTypes");

            migrationBuilder.CreateTable(
                name: "EnumDms",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConnectionInfoID = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumDms", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EnumDms_ConnectionInfos_ConnectionInfoID",
                        column: x => x.ConnectionInfoID,
                        principalTable: "ConnectionInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnumDmValues",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EnumDmID = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumDmValues", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EnumDmValues_EnumDms_EnumDmID",
                        column: x => x.EnumDmID,
                        principalTable: "EnumDms",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserEnumDmValues",
                columns: table => new
                {
                    EnumDmValueID = table.Column<int>(type: "INTEGER", nullable: false),
                    UserID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEnumDmValues", x => new { x.EnumDmValueID, x.UserID });
                    table.ForeignKey(
                        name: "FK_UserEnumDmValues_EnumDmValues_EnumDmValueID",
                        column: x => x.EnumDmValueID,
                        principalTable: "EnumDmValues",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserEnumDmValues_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnumDms_ConnectionInfoID",
                table: "EnumDms",
                column: "ConnectionInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_EnumDmValues_EnumDmID",
                table: "EnumDmValues",
                column: "EnumDmID");

            migrationBuilder.CreateIndex(
                name: "IX_UserEnumDmValues_UserID",
                table: "UserEnumDmValues",
                column: "UserID");
        }
    }
}
