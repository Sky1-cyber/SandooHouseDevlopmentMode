using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sandoohouse.Migrations
{
    /// <inheritdoc />
    public partial class CreateRelationOrderAndOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Brands_BrandId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Menus_MenuItemId",
                table: "OrderItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_Orders_PosOrderId",
                table: "OrderItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem");

            migrationBuilder.RenameTable(
                name: "OrderItem",
                newName: "OrderItems");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_PosOrderId",
                table: "OrderItems",
                newName: "IX_OrderItems_PosOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_MenuItemId",
                table: "OrderItems",
                newName: "IX_OrderItems_MenuItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItem_BrandId",
                table: "OrderItems",
                newName: "IX_OrderItems_BrandId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Brands_BrandId",
                table: "OrderItems",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Menus_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_PosOrderId",
                table: "OrderItems",
                column: "PosOrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Brands_BrandId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Menus_MenuItemId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_PosOrderId",
                table: "OrderItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderItems",
                table: "OrderItems");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "OrderItem");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_PosOrderId",
                table: "OrderItem",
                newName: "IX_OrderItem_PosOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItem",
                newName: "IX_OrderItem_MenuItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderItems_BrandId",
                table: "OrderItem",
                newName: "IX_OrderItem_BrandId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderItem",
                table: "OrderItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Brands_BrandId",
                table: "OrderItem",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Menus_MenuItemId",
                table: "OrderItem",
                column: "MenuItemId",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_Orders_PosOrderId",
                table: "OrderItem",
                column: "PosOrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
