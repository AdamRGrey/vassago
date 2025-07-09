using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace vassago.Migrations
{
    /// <inheritdoc />
    public partial class UnconfinguredWebhooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Webhooks",
                table: "Configurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<string>>(
                name: "Webhooks",
                table: "Configurations",
                type: "text[]",
                nullable: true);
        }
    }
}
