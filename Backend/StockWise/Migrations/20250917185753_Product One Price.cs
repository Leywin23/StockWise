using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockWise.Migrations
{
    /// <inheritdoc />
    public partial class ProductOnePrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SellingPriceAmount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SellingPriceCurrency",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "ShoppingPriceCurrency",
                table: "Products",
                newName: "PriceCurrency");

            migrationBuilder.RenameColumn(
                name: "ShoppingPriceAmount",
                table: "Products",
                newName: "PriceAmount");

            migrationBuilder.RenameColumn(
                name: "CompanySellingPriceCurrency",
                table: "CompanyProducts",
                newName: "CompanyPriceCurrency");

            migrationBuilder.RenameColumn(
                name: "CompanySellingPriceAmount",
                table: "CompanyProducts",
                newName: "CompanygPriceAmount");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriceCurrency",
                table: "Products",
                newName: "ShoppingPriceCurrency");

            migrationBuilder.RenameColumn(
                name: "PriceAmount",
                table: "Products",
                newName: "ShoppingPriceAmount");

            migrationBuilder.RenameColumn(
                name: "CompanygPriceAmount",
                table: "CompanyProducts",
                newName: "CompanySellingPriceAmount");

            migrationBuilder.RenameColumn(
                name: "CompanyPriceCurrency",
                table: "CompanyProducts",
                newName: "CompanySellingPriceCurrency");

            migrationBuilder.AddColumn<decimal>(
                name: "SellingPriceAmount",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SellingPriceCurrency",
                table: "Products",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }
    }
}
