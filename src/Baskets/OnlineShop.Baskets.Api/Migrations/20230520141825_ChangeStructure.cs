using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OnlineShop.Baskets.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChangeStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BasketProducts");

            migrationBuilder.AddColumn<List<int>>(
                name: "BasketProductIds",
                table: "Baskets",
                type: "integer[]",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Baskets",
                keyColumn: "Id",
                keyValue: 1,
                column: "BasketProductIds",
                value: null);

            migrationBuilder.UpdateData(
                table: "Baskets",
                keyColumn: "Id",
                keyValue: 2,
                column: "BasketProductIds",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasketProductIds",
                table: "Baskets");

            migrationBuilder.CreateTable(
                name: "BasketProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BasketId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BasketProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BasketProducts_Baskets_BasketId",
                        column: x => x.BasketId,
                        principalTable: "Baskets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BasketProducts",
                columns: new[] { "Id", "BasketId", "ProductId", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, 1, 5 },
                    { 2, 1, 2, 50 },
                    { 3, 1, 3, 10 },
                    { 4, 2, 4, 100 },
                    { 5, 2, 5, 20 },
                    { 6, 2, 6, 70 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BasketProducts_BasketId",
                table: "BasketProducts",
                column: "BasketId");
        }
    }
}
