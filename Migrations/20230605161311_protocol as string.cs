using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class protocolasstring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_Channels_ProtocolId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Channels_ProtocolId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ChannelUser");

            migrationBuilder.DropIndex(
                name: "IX_Channels_ProtocolId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "ProtocolId",
                table: "Channels");

            migrationBuilder.RenameColumn(
                name: "ProtocolId",
                table: "Users",
                newName: "SeenInChannelId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_ProtocolId",
                table: "Users",
                newName: "IX_Users_SeenInChannelId");

            migrationBuilder.RenameColumn(
                name: "ConnectionToken",
                table: "Channels",
                newName: "Protocol");

            migrationBuilder.AddColumn<string>(
                name: "Protocol",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Channels_SeenInChannelId",
                table: "Users",
                column: "SeenInChannelId",
                principalTable: "Channels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Channels_SeenInChannelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Protocol",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "SeenInChannelId",
                table: "Users",
                newName: "ProtocolId");

            migrationBuilder.RenameIndex(
                name: "IX_Users_SeenInChannelId",
                table: "Users",
                newName: "IX_Users_ProtocolId");

            migrationBuilder.RenameColumn(
                name: "Protocol",
                table: "Channels",
                newName: "ConnectionToken");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Channels",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "ProtocolId",
                table: "Channels",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChannelUser",
                columns: table => new
                {
                    OtherUsersId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeenInChannelsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelUser", x => new { x.OtherUsersId, x.SeenInChannelsId });
                    table.ForeignKey(
                        name: "FK_ChannelUser_Channels_SeenInChannelsId",
                        column: x => x.SeenInChannelsId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelUser_Users_OtherUsersId",
                        column: x => x.OtherUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Channels_ProtocolId",
                table: "Channels",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUser_SeenInChannelsId",
                table: "ChannelUser",
                column: "SeenInChannelsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_Channels_ProtocolId",
                table: "Channels",
                column: "ProtocolId",
                principalTable: "Channels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Channels_ProtocolId",
                table: "Users",
                column: "ProtocolId",
                principalTable: "Channels",
                principalColumn: "Id");
        }
    }
}
