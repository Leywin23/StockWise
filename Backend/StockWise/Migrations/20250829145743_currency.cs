using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockWise.Migrations
{
    /// <inheritdoc />
    public partial class currency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShoppingPrice",
                table: "Products",
                newName: "ShoppingPriceAmount");

            migrationBuilder.RenameColumn(
                name: "SellingPrice",
                table: "Products",
                newName: "SellingPriceAmount");

            migrationBuilder.RenameColumn(
                name: "ShoppingPrice",
                table: "CompanyProducts",
                newName: "CompanyShoppingPriceAmount");

            migrationBuilder.RenameColumn(
                name: "SellingPrice",
                table: "CompanyProducts",
                newName: "CompanySellingPriceAmount");

            migrationBuilder.AddColumn<string>(
                name: "SellingPriceCurrency",
                table: "Products",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShoppingPriceCurrency",
                table: "Products",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanySellingPriceCurrency",
                table: "CompanyProducts",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyShoppingPriceCurrency",
                table: "CompanyProducts",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellingPriceCurrency",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShoppingPriceCurrency",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CompanySellingPriceCurrency",
                table: "CompanyProducts");

            migrationBuilder.DropColumn(
                name: "CompanyShoppingPriceCurrency",
                table: "CompanyProducts");

            migrationBuilder.RenameColumn(
                name: "ShoppingPriceAmount",
                table: "Products",
                newName: "ShoppingPrice");

            migrationBuilder.RenameColumn(
                name: "SellingPriceAmount",
                table: "Products",
                newName: "SellingPrice");

            migrationBuilder.RenameColumn(
                name: "CompanyShoppingPriceAmount",
                table: "CompanyProducts",
                newName: "ShoppingPrice");

            migrationBuilder.RenameColumn(
                name: "CompanySellingPriceAmount",
                table: "CompanyProducts",
                newName: "SellingPrice");
        }
    }
}
