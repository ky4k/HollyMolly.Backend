using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HM.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedNeePostContactPersontable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "NewPostContactPersons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phones",
                table: "NewPostContactPersons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "NewPostContactPersons");

            migrationBuilder.DropColumn(
                name: "Phones",
                table: "NewPostContactPersons");
        }
    }
}
