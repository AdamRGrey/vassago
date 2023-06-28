using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class NotionOfUserAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_AuthorId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Channels_SeenInChannelId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_SeenInChannelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBot",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PermissionTags",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Protocol",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SeenInChannelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxAttachmentBytes",
                table: "PermissionSettings",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    IsBot = table.Column<bool>(type: "boolean", nullable: false),
                    SeenInChannelId = table.Column<Guid>(type: "uuid", nullable: true),
                    PermissionTags = table.Column<int[]>(type: "integer[]", nullable: true),
                    Protocol = table.Column<string>(type: "text", nullable: true),
                    IsUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Channels_SeenInChannelId",
                        column: x => x.SeenInChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accounts_Users_IsUserId",
                        column: x => x.IsUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_IsUserId",
                table: "Accounts",
                column: "IsUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SeenInChannelId",
                table: "Accounts",
                column: "SeenInChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Accounts_AuthorId",
                table: "Messages",
                column: "AuthorId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Accounts_AuthorId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExternalId",
                table: "Users",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBot",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int[]>(
                name: "PermissionTags",
                table: "Users",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Protocol",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SeenInChannelId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "MaxAttachmentBytes",
                table: "PermissionSettings",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SeenInChannelId",
                table: "Users",
                column: "SeenInChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_AuthorId",
                table: "Messages",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Channels_SeenInChannelId",
                table: "Users",
                column: "SeenInChannelId",
                principalTable: "Channels",
                principalColumn: "Id");
        }
    }
}
