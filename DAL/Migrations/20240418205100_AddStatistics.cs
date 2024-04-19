using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HM.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductInstance_Discount_DiscountId",
                table: "ProductInstance");

            migrationBuilder.DropTable(
                name: "Discount");

            migrationBuilder.DropIndex(
                name: "IX_ProductInstance_DiscountId",
                table: "ProductInstance");

            migrationBuilder.DropColumn(
                name: "DiscountId",
                table: "ProductInstance");

            migrationBuilder.RenameColumn(
                name: "NumberPurchases",
                table: "ProductStatistics",
                newName: "Year");

            migrationBuilder.AddColumn<int>(
                name: "Day",
                table: "ProductStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "ProductStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberFeedbacks",
                table: "ProductStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "AbsoluteDiscount",
                table: "ProductInstance",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsNewCollection",
                table: "ProductInstance",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "ProductInstance",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentageDiscount",
                table: "ProductInstance",
                type: "decimal(8,4)",
                precision: 8,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ProductInstance",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "ProductFeedback",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "OrderRecord",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ProductInstanceStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductStatisticsId = table.Column<int>(type: "int", nullable: false),
                    ProductInstanceId = table.Column<int>(type: "int", nullable: false),
                    NumberOfPurchases = table.Column<int>(type: "int", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductInstanceStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductInstanceStatistics_ProductInstance_ProductInstanceId",
                        column: x => x.ProductInstanceId,
                        principalTable: "ProductInstance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductInstanceStatistics_ProductStatistics_ProductStatisticsId",
                        column: x => x.ProductStatisticsId,
                        principalTable: "ProductStatistics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductInstanceStatistics_ProductInstanceId",
                table: "ProductInstanceStatistics",
                column: "ProductInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductInstanceStatistics_ProductStatisticsId",
                table: "ProductInstanceStatistics",
                column: "ProductStatisticsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductInstanceStatistics");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "ProductStatistics");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "ProductStatistics");

            migrationBuilder.DropColumn(
                name: "NumberFeedbacks",
                table: "ProductStatistics");

            migrationBuilder.DropColumn(
                name: "AbsoluteDiscount",
                table: "ProductInstance");

            migrationBuilder.DropColumn(
                name: "IsNewCollection",
                table: "ProductInstance");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "ProductInstance");

            migrationBuilder.DropColumn(
                name: "PercentageDiscount",
                table: "ProductInstance");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProductInstance");

            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "ProductFeedback");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "OrderRecord");

            migrationBuilder.RenameColumn(
                name: "Year",
                table: "ProductStatistics",
                newName: "NumberPurchases");

            migrationBuilder.AddColumn<int>(
                name: "DiscountId",
                table: "ProductInstance",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Discount",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AbsoluteDiscount = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    PercentageDiscount = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discount", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductInstance_DiscountId",
                table: "ProductInstance",
                column: "DiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductInstance_Discount_DiscountId",
                table: "ProductInstance",
                column: "DiscountId",
                principalTable: "Discount",
                principalColumn: "Id");
        }
    }
}
