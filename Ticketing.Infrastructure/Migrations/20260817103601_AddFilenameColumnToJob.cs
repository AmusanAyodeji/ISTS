using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFilenameColumnToJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Jobs");
        }
    }
}
