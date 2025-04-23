using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class UAC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UACs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UACs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccountUAC",
                columns: table => new
                {
                    AccountInChannelsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UACsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountUAC", x => new { x.AccountInChannelsId, x.UACsId });
                    table.ForeignKey(
                        name: "FK_AccountUAC_Accounts_AccountInChannelsId",
                        column: x => x.AccountInChannelsId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountUAC_UACs_UACsId",
                        column: x => x.UACsId,
                        principalTable: "UACs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelUAC",
                columns: table => new
                {
                    ChannelsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UACsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelUAC", x => new { x.ChannelsId, x.UACsId });
                    table.ForeignKey(
                        name: "FK_ChannelUAC_Channels_ChannelsId",
                        column: x => x.ChannelsId,
                        principalTable: "Channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelUAC_UACs_UACsId",
                        column: x => x.UACsId,
                        principalTable: "UACs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UACUser",
                columns: table => new
                {
                    UACsId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UACUser", x => new { x.UACsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UACUser_UACs_UACsId",
                        column: x => x.UACsId,
                        principalTable: "UACs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UACUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountUAC_UACsId",
                table: "AccountUAC",
                column: "UACsId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUAC_UACsId",
                table: "ChannelUAC",
                column: "UACsId");

            migrationBuilder.CreateIndex(
                name: "IX_UACUser_UsersId",
                table: "UACUser",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountUAC");

            migrationBuilder.DropTable(
                name: "ChannelUAC");

            migrationBuilder.DropTable(
                name: "UACUser");

            migrationBuilder.DropTable(
                name: "UACs");
        }
    }
}
