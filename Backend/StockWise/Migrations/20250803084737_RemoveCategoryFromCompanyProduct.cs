using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockWise.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryFromCompanyProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyProducts_Categories_CategoryId",
                table: "CompanyProducts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyProducts_CategoryId",
                table: "CompanyProducts");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "CompanyProducts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "CompanyProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProducts_CategoryId",
                table: "CompanyProducts",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanyProducts_Categories_CategoryId",
                table: "CompanyProducts",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
