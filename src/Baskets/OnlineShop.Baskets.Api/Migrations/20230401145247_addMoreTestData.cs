using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnlineShop.Baskets.Api.Migrations
{
    /// <inheritdoc />
    public partial class addMoreTestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BasketProducts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BasketId", "ProductId", "Quantity" },
                values: new object[] { 1, 2, 50 });

            migrationBuilder.InsertData(
                table: "BasketProducts",
                columns: new[] { "Id", "BasketId", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 3, 1, 3, 10 },
                    { 4, 2, 4, 100 },
                    { 5, 2, 5, 20 },
                    { 6, 2, 6, 70 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BasketProducts",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BasketProducts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BasketProducts",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BasketProducts",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "BasketProducts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BasketId", "ProductId", "Quantity" },
                values: new object[] { 2, 1, 10 });
        }
    }
}
