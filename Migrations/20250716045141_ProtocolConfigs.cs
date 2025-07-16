using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class ProtocolConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscordTokens",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "SetupDiscordSlashCommands",
                table: "Configurations");

            migrationBuilder.DropColumn(
                name: "TwitchConfigs",
                table: "Configurations");

            migrationBuilder.CreateTable(
                name: "ProtocolConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Discriminator = table.Column<string>(type: "text", nullable: false),
                    token = table.Column<string>(type: "text", nullable: true),
                    SetupSlashCommands = table.Column<bool>(type: "boolean", nullable: true),
                    username = table.Column<string>(type: "text", nullable: true),
                    oauth = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolConfigurations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProtocolConfigurations");

            migrationBuilder.AddColumn<List<string>>(
                name: "DiscordTokens",
                table: "Configurations",
                type: "text[]",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SetupDiscordSlashCommands",
                table: "Configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "TwitchConfigs",
                table: "Configurations",
                type: "text[]",
                nullable: true);
        }
    }
}
