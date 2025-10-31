using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockWise.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleteFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyProduct_CompanyId_EAN",
                table: "CompanyProducts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyProduct_CompanyId_Name",
                table: "CompanyProducts");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProduct_CompanyId_EAN",
                table: "CompanyProducts",
                columns: new[] { "CompanyId", "EAN" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProduct_CompanyId_Name",
                table: "CompanyProducts",
                columns: new[] { "CompanyId", "CompanyProductName" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyProduct_CompanyId_EAN",
                table: "CompanyProducts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyProduct_CompanyId_Name",
                table: "CompanyProducts");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProduct_CompanyId_EAN",
                table: "CompanyProducts",
                columns: new[] { "CompanyId", "EAN" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyProduct_CompanyId_Name",
                table: "CompanyProducts",
                columns: new[] { "CompanyId", "CompanyProductName" },
                unique: true);
        }
    }
}
