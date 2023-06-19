using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class DistinguishUsersAndAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "PermissionTags",
                table: "Users",
                type: "integer[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PermissionTags",
                table: "Users");
        }
    }
}
