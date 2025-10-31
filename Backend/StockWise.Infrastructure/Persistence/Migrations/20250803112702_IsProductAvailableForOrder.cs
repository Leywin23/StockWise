using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockWise.Migrations
{
    /// <inheritdoc />
    public partial class IsProductAvailableForOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Orders_OrderId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_OrderId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Products");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForOrder",
                table: "CompanyProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "CompanyProducts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProducts_OrderId",
                table: "CompanyProducts",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyProducts_Orders_OrderId",
                table: "CompanyProducts",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyProducts_Orders_OrderId",
                table: "CompanyProducts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyProducts_OrderId",
                table: "CompanyProducts");

            migrationBuilder.DropColumn(
                name: "IsAvailableForOrder",
                table: "CompanyProducts");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "CompanyProducts");

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_OrderId",
                table: "Products",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Orders_OrderId",
                table: "Products",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");
        }
    }
}
