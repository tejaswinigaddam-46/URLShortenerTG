using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Database.Migrations
{
    [Microsoft.EntityFrameworkCore.Migrations.Migration("20251207082000_ShortCodeLength7")]
    public partial class ShortCodeLength7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShortCode",
                table: "UrlMappings",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                collation: "C",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldCollation: "C");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ShortCode",
                table: "UrlMappings",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                collation: "C",
                oldClrType: typeof(string),
                oldType: "character varying(7)",
                oldMaxLength: 7,
                oldCollation: "C");
        }
    }
}
