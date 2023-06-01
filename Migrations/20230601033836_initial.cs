using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PermissionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaxAttachmentBytes = table.Column<long>(type: "bigint", nullable: true),
                    MaxTextChars = table.Column<long>(type: "bigint", nullable: true),
                    LinksAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    LewdnessFilterLevel = table.Column<int>(type: "integer", nullable: true),
                    MeannessFilterLevel = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    IsDM = table.Column<bool>(type: "boolean", nullable: false),
                    PermissionsOverridesId = table.Column<int>(type: "integer", nullable: true),
                    ParentChannelId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProtocolId = table.Column<Guid>(type: "uuid", nullable: true),
                    Discriminator = table.Column<string>(type: "text", nullable: false),
                    ConnectionToken = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Channels_Channels_ParentChannelId",
                        column: x => x.ParentChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Channels_Channels_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "Channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Channels_PermissionSettings_PermissionsOverridesId",
                        column: x => x.PermissionsOverridesId,
                        principalTable: "PermissionSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    IsBot = table.Column<bool>(type: "boolean", nullable: false),
                    ProtocolId = table.Column<Guid>(type: "uuid", nullable: true),
                    External = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Channels_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "Channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    MentionsMe = table.Column<bool>(type: "boolean", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ActedOn = table.Column<bool>(type: "boolean", nullable: false),
                    ExternalRepresentation = table.Column<string>(type: "text", nullable: true),
                    AuthorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChannelId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<byte[]>(type: "bytea", nullable: true),
                    Filename = table.Column<string>(type: "text", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_MessageId",
                table: "Attachments",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_ParentChannelId",
                table: "Channels",
                column: "ParentChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_PermissionsOverridesId",
                table: "Channels",
                column: "PermissionsOverridesId");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_ProtocolId",
                table: "Channels",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelUser_SeenInChannelsId",
                table: "ChannelUser",
                column: "SeenInChannelsId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_AuthorId",
                table: "Messages",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelId",
                table: "Messages",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProtocolId",
                table: "Users",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserId",
                table: "Users",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "ChannelUser");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Channels");

            migrationBuilder.DropTable(
                name: "PermissionSettings");
        }
    }
}
