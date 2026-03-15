using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandoohouse.Migrations
{
    /// <inheritdoc />
    public partial class CreateMenuSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Admins_CreatedBy",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Categories_CategoryId",
                table: "Menus");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "Menus",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Menus",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Admins_CreatedBy",
                table: "Menus",
                column: "CreatedBy",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Categories_CategoryId",
                table: "Menus",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Admins_CreatedBy",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Categories_CategoryId",
                table: "Menus");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedBy",
                table: "Menus",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Menus",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Admins_CreatedBy",
                table: "Menus",
                column: "CreatedBy",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Categories_CategoryId",
                table: "Menus",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
