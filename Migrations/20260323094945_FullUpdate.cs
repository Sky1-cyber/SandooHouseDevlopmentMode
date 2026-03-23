using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandoohouse.Migrations
{
    /// <inheritdoc />
    public partial class FullUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Admins_CreatedById",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Admins_CreatedBy",
                table: "Menus");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Admins_CreatedById",
                table: "Categories",
                column: "CreatedById",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Admins_CreatedBy",
                table: "Menus",
                column: "CreatedBy",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Admins_CreatedById",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Admins_CreatedBy",
                table: "Menus");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Admins_CreatedById",
                table: "Categories",
                column: "CreatedById",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Admins_CreatedBy",
                table: "Menus",
                column: "CreatedBy",
                principalTable: "Admins",
                principalColumn: "Id");
        }
    }
}
