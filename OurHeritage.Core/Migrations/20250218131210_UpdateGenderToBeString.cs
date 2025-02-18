using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OurHeritage.Core.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGenderToBeString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHandiCraft_HandiCrafts_HandiCraftId",
                table: "OrderHandiCraft");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHandiCraft_Orders_OrderId",
                table: "OrderHandiCraft");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "CulturalArticles",
                type: "nvarchar(280)",
                maxLength: 280,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHandiCraft_HandiCrafts_HandiCraftId",
                table: "OrderHandiCraft",
                column: "HandiCraftId",
                principalTable: "HandiCrafts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHandiCraft_Orders_OrderId",
                table: "OrderHandiCraft",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderHandiCraft_HandiCrafts_HandiCraftId",
                table: "OrderHandiCraft");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderHandiCraft_Orders_OrderId",
                table: "OrderHandiCraft");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "CulturalArticles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(280)",
                oldMaxLength: 280);

            migrationBuilder.AlterColumn<int>(
                name: "Gender",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHandiCraft_HandiCrafts_HandiCraftId",
                table: "OrderHandiCraft",
                column: "HandiCraftId",
                principalTable: "HandiCrafts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderHandiCraft_Orders_OrderId",
                table: "OrderHandiCraft",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
