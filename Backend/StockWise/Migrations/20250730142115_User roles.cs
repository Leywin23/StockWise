using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StockWise.Migrations
{
    /// <inheritdoc />
    public partial class Userroles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Companies_BuyerId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BuyerId1",
                table: "Orders");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "32844c0b-c292-416a-9d18-06d1b75370da");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "344414ab-028c-407c-931d-dc22ad607eb7");

            migrationBuilder.DropColumn(
                name: "BuyerId1",
                table: "Orders");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", null, "Manager", "MANAGER" },
                    { "2", null, "Worker", "Worker" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SellerId",
                table: "Orders",
                column: "SellerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Companies_SellerId",
                table: "Orders",
                column: "SellerId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Companies_SellerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SellerId",
                table: "Orders");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2");

            migrationBuilder.AddColumn<int>(
                name: "BuyerId1",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "32844c0b-c292-416a-9d18-06d1b75370da", null, "Manager", "MANAGER" },
                    { "344414ab-028c-407c-931d-dc22ad607eb7", null, "Worker", "Worker" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BuyerId1",
                table: "Orders",
                column: "BuyerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Companies_BuyerId1",
                table: "Orders",
                column: "BuyerId1",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
