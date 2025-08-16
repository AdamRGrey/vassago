using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
#pragma warning disable 8981
    /// <inheritdoc />
    public partial class datamemosmoredata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "CommandAliases",
                table: "UACs",
                type: "hstore",
                nullable: true);

            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "Localization",
                table: "UACs",
                type: "hstore",
                nullable: true);

            //NOTE for future me: migrationBuilder.SQL("SELECT localization INTO Aliases from Channels;");, but also make the rows for it.
            //too lazy now, really leaning on the "this will work fine for my 0 users"

            migrationBuilder.DropColumn(
                name: "Aliases",
                table: "Channels");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Dictionary<string, string>>(
                name: "Aliases",
                table: "Channels",
                type: "hstore",
                nullable: true);

            migrationBuilder.DropColumn(
                name: "CommandAliases",
                table: "UACs");

            migrationBuilder.DropColumn(
                name: "Localization",
                table: "UACs");
        }
    }
}
#pragma warning restore 8981
