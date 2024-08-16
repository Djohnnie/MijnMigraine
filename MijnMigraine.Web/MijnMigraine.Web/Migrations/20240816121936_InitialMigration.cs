using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MijnMigraine.Web.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MigraineEntries",
            columns: table => new
            {
                SysId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DateOfOccurrence = table.Column<DateTime>(type: "datetime2", nullable: false),
                Severity = table.Column<byte>(type: "tinyint", nullable: false),
                Duration = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                AdditionalInfo = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MigraineEntries", x => x.SysId)
                    .Annotation("SqlServer:Clustered", true);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "MigraineEntries");
    }
}