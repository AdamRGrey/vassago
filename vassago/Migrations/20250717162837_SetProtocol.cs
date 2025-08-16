using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class SetProtocol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Protocol",
                table: "ProtocolConfigurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SelfChannelId",
                table: "ProtocolConfigurations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolConfigurations_SelfChannelId",
                table: "ProtocolConfigurations",
                column: "SelfChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProtocolConfigurations_Channels_SelfChannelId",
                table: "ProtocolConfigurations",
                column: "SelfChannelId",
                principalTable: "Channels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProtocolConfigurations_Channels_SelfChannelId",
                table: "ProtocolConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_ProtocolConfigurations_SelfChannelId",
                table: "ProtocolConfigurations");

            migrationBuilder.DropColumn(
                name: "Protocol",
                table: "ProtocolConfigurations");

            migrationBuilder.DropColumn(
                name: "SelfChannelId",
                table: "ProtocolConfigurations");
        }
    }
}
