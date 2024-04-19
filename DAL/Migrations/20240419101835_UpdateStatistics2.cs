using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HM.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStatistics2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductInstanceProductInstanceStatistics");

            migrationBuilder.DropTable(
                name: "ProductProductStatistics");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ProductStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductStatistics_ProductId",
                table: "ProductStatistics",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductInstanceStatistics_ProductInstanceId",
                table: "ProductInstanceStatistics",
                column: "ProductInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInstanceStatistics_ProductInstance_ProductInstanceId",
                table: "ProductInstanceStatistics",
                column: "ProductInstanceId",
                principalTable: "ProductInstance",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductStatistics_Products_ProductId",
                table: "ProductStatistics",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductInstanceStatistics_ProductInstance_ProductInstanceId",
                table: "ProductInstanceStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductStatistics_Products_ProductId",
                table: "ProductStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ProductStatistics_ProductId",
                table: "ProductStatistics");

            migrationBuilder.DropIndex(
                name: "IX_ProductInstanceStatistics_ProductInstanceId",
                table: "ProductInstanceStatistics");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductStatistics");

            migrationBuilder.CreateTable(
                name: "ProductInstanceProductInstanceStatistics",
                columns: table => new
                {
                    ProductInstanceStatisticsId = table.Column<int>(type: "int", nullable: false),
                    ProductInstancesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductInstanceProductInstanceStatistics", x => new { x.ProductInstanceStatisticsId, x.ProductInstancesId });
                    table.ForeignKey(
                        name: "FK_ProductInstanceProductInstanceStatistics_ProductInstanceStatistics_ProductInstanceStatisticsId",
                        column: x => x.ProductInstanceStatisticsId,
                        principalTable: "ProductInstanceStatistics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductInstanceProductInstanceStatistics_ProductInstance_ProductInstancesId",
                        column: x => x.ProductInstancesId,
                        principalTable: "ProductInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductProductStatistics",
                columns: table => new
                {
                    ProductStatisticsId = table.Column<int>(type: "int", nullable: false),
                    ProductsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProductStatistics", x => new { x.ProductStatisticsId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ProductProductStatistics_ProductStatistics_ProductStatisticsId",
                        column: x => x.ProductStatisticsId,
                        principalTable: "ProductStatistics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductProductStatistics_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductInstanceProductInstanceStatistics_ProductInstancesId",
                table: "ProductInstanceProductInstanceStatistics",
                column: "ProductInstancesId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProductStatistics_ProductsId",
                table: "ProductProductStatistics",
                column: "ProductsId");
        }
    }
}
