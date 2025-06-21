using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
#pragma warning disable 8981
    /// <inheritdoc />
    public partial class locales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Localization",
                table: "UACs",
                newName: "Translations");

            migrationBuilder.RenameColumn(
                name: "CommandAliases",
                table: "UACs",
                newName: "CommandAlterations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Translations",
                table: "UACs",
                newName: "Localization");

            migrationBuilder.RenameColumn(
                name: "CommandAlterations",
                table: "UACs",
                newName: "CommandAliases");
        }
    }
#pragma warning restore 8981
}
