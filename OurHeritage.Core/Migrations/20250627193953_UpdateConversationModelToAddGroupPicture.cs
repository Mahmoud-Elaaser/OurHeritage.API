using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OurHeritage.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConversationModelToAddGroupPicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GroupPicture",
                table: "Conversations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupPicture",
                table: "Conversations");
        }
    }
}
