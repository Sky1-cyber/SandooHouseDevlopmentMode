using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandoohouse.Migrations
{
    /// <inheritdoc />
    public partial class RelationBrandAndCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BrandId",
                table: "Categories",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_BrandId",
                table: "Categories",
                column: "BrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Brands_BrandId",
                table: "Categories",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Brands_BrandId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_BrandId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "BrandId",
                table: "Categories");
        }
    }
}
