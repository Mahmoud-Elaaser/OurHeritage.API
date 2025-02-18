using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OurHeritage.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHandiCraftEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "HandiCrafts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "HandiCrafts");
        }
    }
}
