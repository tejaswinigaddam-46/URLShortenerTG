using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UrlMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShortCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, collation: "C"),
                    LongUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Clicks = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlMappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UrlMappings_ShortCode",
                table: "UrlMappings",
                column: "ShortCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UrlMappings");
        }
    }
}
