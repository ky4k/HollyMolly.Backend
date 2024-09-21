using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HM.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTypeOfCostOnSiteInNewPostInternetDocumentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "CostOnSite",
                table: "NewPostInternetDocuments",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostOnSite",
                table: "NewPostInternetDocuments");
        }
    }
}
