using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockWise.Migrations
{
    /// <inheritdoc />
    public partial class OrderProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanyProducts_Orders_OrderId",
                table: "CompanyProducts");

            migrationBuilder.DropIndex(
                name: "IX_CompanyProducts_OrderId",
                table: "CompanyProducts");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "CompanyProducts");

            migrationBuilder.CreateTable(
                name: "OrderProduct",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CompanyProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderProduct", x => new { x.OrderId, x.CompanyProductId });
                    table.ForeignKey(
                        name: "FK_OrderProduct_CompanyProducts_CompanyProductId",
                        column: x => x.CompanyProductId,
                        principalTable: "CompanyProducts",
                        principalColumn: "CompanyProductId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderProduct_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderProduct_CompanyProductId",
                table: "OrderProduct",
                column: "CompanyProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderProduct");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
    }
}
