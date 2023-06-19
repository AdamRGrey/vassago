using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class permissionsmove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_PermissionSettings_PermissionsOverridesId",
                table: "Channels");

            migrationBuilder.RenameColumn(
                name: "PermissionsOverridesId",
                table: "Channels",
                newName: "PermissionsId");

            migrationBuilder.RenameIndex(
                name: "IX_Channels_PermissionsOverridesId",
                table: "Channels",
                newName: "IX_Channels_PermissionsId");

            migrationBuilder.AddColumn<bool>(
                name: "ReactionsPossible",
                table: "PermissionSettings",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_PermissionSettings_PermissionsId",
                table: "Channels",
                column: "PermissionsId",
                principalTable: "PermissionSettings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_PermissionSettings_PermissionsId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "ReactionsPossible",
                table: "PermissionSettings");

            migrationBuilder.RenameColumn(
                name: "PermissionsId",
                table: "Channels",
                newName: "PermissionsOverridesId");

            migrationBuilder.RenameIndex(
                name: "IX_Channels_PermissionsId",
                table: "Channels",
                newName: "IX_Channels_PermissionsOverridesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_PermissionSettings_PermissionsOverridesId",
                table: "Channels",
                column: "PermissionsOverridesId",
                principalTable: "PermissionSettings",
                principalColumn: "Id");
        }
    }
}
