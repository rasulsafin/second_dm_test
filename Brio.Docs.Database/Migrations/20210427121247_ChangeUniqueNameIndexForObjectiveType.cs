using Microsoft.EntityFrameworkCore.Migrations;

namespace Brio.Docs.Database.Migrations
{
    public partial class ChangeUniqueNameIndexForObjectiveType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ObjectiveTypes_Name",
                table: "ObjectiveTypes");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveTypes_Name_ConnectionTypeID",
                table: "ObjectiveTypes",
                columns: new[] { "Name", "ConnectionTypeID" },
                unique: true);

            migrationBuilder.Sql(
                @"CREATE TRIGGER UniqueNameInsertChecker
    BEFORE INSERT
    ON ObjectiveTypes
    FOR EACH ROW
BEGIN
    SELECT CASE
               WHEN EXISTS(SELECT *
                           FROM ObjectiveTypes
                           WHERE ((new.ConnectionTypeID IS NULL AND ConnectionTypeID IS NULL) OR
                                  new.ConnectionTypeID = ConnectionTypeID)
                             AND ((new.Name IS NULL AND Name IS NULL) OR new.Name = Name))
                   THEN RAISE(ABORT, 'Name must be unique for connection') END;
END; ");
            migrationBuilder.Sql(
                @"CREATE TRIGGER UniqueNameUpdateChecker
    BEFORE UPDATE
    ON ObjectiveTypes
    FOR EACH ROW
BEGIN
    SELECT CASE
               WHEN EXISTS(SELECT *
                           FROM ObjectiveTypes
                           WHERE ((new.ConnectionTypeID IS NULL AND ConnectionTypeID IS NULL) OR
                                  new.ConnectionTypeID = ConnectionTypeID)
                             AND ((new.Name IS NULL AND Name IS NULL) OR new.Name = Name))
                   THEN RAISE(ABORT, 'Name must be unique for connection') END;
END; ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ObjectiveTypes_Name_ConnectionTypeID",
                table: "ObjectiveTypes");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectiveTypes_Name",
                table: "ObjectiveTypes",
                column: "Name",
                unique: true);
            
            migrationBuilder.Sql(@"DROP TRIGGER UniqueNameInsertChecker");
            migrationBuilder.Sql(@"DROP TRIGGER UniqueNameUpdateChecker");
        }
    }
}
