﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class FeaturePermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Channels_PermissionSettings_PermissionsId",
                table: "Channels");

            migrationBuilder.DropTable(
                name: "PermissionSettings");

            migrationBuilder.DropColumn(
                name: "PermissionTags",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PermissionTags",
                table: "Accounts");

            migrationBuilder.AddColumn<Guid>(
                name: "FeaturePermissionId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FeaturePermissionId",
                table: "Channels",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FeaturePermissionId",
                table: "Accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChannelPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaxAttachmentBytes = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    MaxTextChars = table.Column<long>(type: "bigint", nullable: true),
                    LinksAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    ReactionsPossible = table.Column<bool>(type: "boolean", nullable: true),
                    LewdnessFilterLevel = table.Column<int>(type: "integer", nullable: true),
                    MeannessFilterLevel = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeaturePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InternalName = table.Column<string>(type: "text", nullable: true),
                    InternalTag = table.Column<int>(type: "integer", nullable: true),
                    Inheritable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeaturePermissions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_FeaturePermissionId",
                table: "Users",
                column: "FeaturePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Channels_FeaturePermissionId",
                table: "Channels",
                column: "FeaturePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_FeaturePermissionId",
                table: "Accounts",
                column: "FeaturePermissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_FeaturePermissions_FeaturePermissionId",
                table: "Accounts",
                column: "FeaturePermissionId",
                principalTable: "FeaturePermissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_ChannelPermissions_PermissionsId",
                table: "Channels",
                column: "PermissionsId",
                principalTable: "ChannelPermissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_FeaturePermissions_FeaturePermissionId",
                table: "Channels",
                column: "FeaturePermissionId",
                principalTable: "FeaturePermissions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_FeaturePermissions_FeaturePermissionId",
                table: "Users",
                column: "FeaturePermissionId",
                principalTable: "FeaturePermissions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_FeaturePermissions_FeaturePermissionId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Channels_ChannelPermissions_PermissionsId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_Channels_FeaturePermissions_FeaturePermissionId",
                table: "Channels");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_FeaturePermissions_FeaturePermissionId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ChannelPermissions");

            migrationBuilder.DropTable(
                name: "FeaturePermissions");

            migrationBuilder.DropIndex(
                name: "IX_Users_FeaturePermissionId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Channels_FeaturePermissionId",
                table: "Channels");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_FeaturePermissionId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "FeaturePermissionId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FeaturePermissionId",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "FeaturePermissionId",
                table: "Accounts");

            migrationBuilder.AddColumn<int[]>(
                name: "PermissionTags",
                table: "Users",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "PermissionTags",
                table: "Accounts",
                type: "integer[]",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PermissionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LewdnessFilterLevel = table.Column<int>(type: "integer", nullable: true),
                    LinksAllowed = table.Column<bool>(type: "boolean", nullable: true),
                    MaxAttachmentBytes = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    MaxTextChars = table.Column<long>(type: "bigint", nullable: true),
                    MeannessFilterLevel = table.Column<int>(type: "integer", nullable: true),
                    ReactionsPossible = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionSettings", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Channels_PermissionSettings_PermissionsId",
                table: "Channels",
                column: "PermissionsId",
                principalTable: "PermissionSettings",
                principalColumn: "Id");
        }
    }
}
