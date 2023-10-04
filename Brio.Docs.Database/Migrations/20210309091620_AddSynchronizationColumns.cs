using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class AddSynchronizationColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectItems");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Items",
                newName: "RelativePath");

            migrationBuilder.RenameColumn(
                name: "ExternalItemId",
                table: "Items",
                newName: "ExternalID");

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "Projects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSynchronized",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SynchronizationMateID",
                table: "Projects",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

            migrationBuilder.AddColumn<string>(
                name: "ExternalID",
                table: "Objectives",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSynchronized",
                table: "Objectives",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SynchronizationMateID",
                table: "Objectives",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Objectives",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

            migrationBuilder.AddColumn<bool>(
                name: "IsSynchronized",
                table: "Items",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProjectID",
                table: "Items",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SynchronizationMateID",
                table: "Items",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Items",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

            migrationBuilder.AddColumn<bool>(
                name: "IsSynchronized",
                table: "DynamicFields",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SynchronizationMateID",
                table: "DynamicFields",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DynamicFields",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc));

            migrationBuilder.CreateTable(
                name: "Synchronizations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Synchronizations", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SynchronizationMateID",
                table: "Projects",
                column: "SynchronizationMateID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Objectives_SynchronizationMateID",
                table: "Objectives",
                column: "SynchronizationMateID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ProjectID",
                table: "Items",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SynchronizationMateID",
                table: "Items",
                column: "SynchronizationMateID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFields_SynchronizationMateID",
                table: "DynamicFields",
                column: "SynchronizationMateID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFields_DynamicFields_SynchronizationMateID",
                table: "DynamicFields",
                column: "SynchronizationMateID",
                principalTable: "DynamicFields",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Items_SynchronizationMateID",
                table: "Items",
                column: "SynchronizationMateID",
                principalTable: "Items",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Projects_ProjectID",
                table: "Items",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Objectives_Objectives_SynchronizationMateID",
                table: "Objectives",
                column: "SynchronizationMateID",
                principalTable: "Objectives",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Projects_SynchronizationMateID",
                table: "Projects",
                column: "SynchronizationMateID",
                principalTable: "Projects",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFields_DynamicFields_SynchronizationMateID",
                table: "DynamicFields");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Items_SynchronizationMateID",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Projects_ProjectID",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Objectives_Objectives_SynchronizationMateID",
                table: "Objectives");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Projects_SynchronizationMateID",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "Synchronizations");

            migrationBuilder.DropIndex(
                name: "IX_Projects_SynchronizationMateID",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Objectives_SynchronizationMateID",
                table: "Objectives");

            migrationBuilder.DropIndex(
                name: "IX_Items_ProjectID",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_SynchronizationMateID",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFields_SynchronizationMateID",
                table: "DynamicFields");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsSynchronized",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SynchronizationMateID",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ExternalID",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "IsSynchronized",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "SynchronizationMateID",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Objectives");

            migrationBuilder.DropColumn(
                name: "IsSynchronized",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ProjectID",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "SynchronizationMateID",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "IsSynchronized",
                table: "DynamicFields");

            migrationBuilder.DropColumn(
                name: "SynchronizationMateID",
                table: "DynamicFields");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DynamicFields");

            migrationBuilder.RenameColumn(
                name: "RelativePath",
                table: "Items",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ExternalID",
                table: "Items",
                newName: "ExternalItemId");

            migrationBuilder.CreateTable(
                name: "ProjectItems",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectItems", x => new { x.ItemID, x.ProjectID });
                    table.ForeignKey(
                        name: "FK_ProjectItems_Items_ItemID",
                        column: x => x.ItemID,
                        principalTable: "Items",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectItems_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItems_ProjectID",
                table: "ProjectItems",
                column: "ProjectID");
        }
    }
}
