using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OurHeritage.Core.Migrations
{
    /// <inheritdoc />
    public partial class DeleteStripeClientSecretFromOrderModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeClientSecret",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripeClientSecret",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
