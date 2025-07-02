using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class ConfigInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscordTokens = table.Column<List<string>>(type: "text[]", nullable: true),
                    TwitchConfigs = table.Column<List<string>>(type: "text[]", nullable: true),
                    ExchangePairsLocation = table.Column<string>(type: "text", nullable: true),
                    SetupDiscordSlashCommands = table.Column<bool>(type: "boolean", nullable: false),
                    Webhooks = table.Column<List<string>>(type: "text[]", nullable: true),
                    KafkaBootstrap = table.Column<string>(type: "text", nullable: true),
                    KafkaName = table.Column<string>(type: "text", nullable: true),
                    reportedApiUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configurations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configurations");
        }
    }
}
