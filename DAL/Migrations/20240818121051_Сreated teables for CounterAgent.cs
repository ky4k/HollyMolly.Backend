using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HM.DAL.Migrations
{
    /// <inheritdoc />
    public partial class СreatedteablesforCounterAgent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewPostCounterAgents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ref = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CounterpartyId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnershipFormId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnershipFormDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EDRPOU = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CounterpartyType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewPostCounterAgents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewPostContactPersons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CounterpartyRef = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewPostContactPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewPostContactPersons_NewPostCounterAgents_CounterpartyRef",
                        column: x => x.CounterpartyRef,
                        principalTable: "NewPostCounterAgents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewPostContactPersons_CounterpartyRef",
                table: "NewPostContactPersons",
                column: "CounterpartyRef");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewPostContactPersons");

            migrationBuilder.DropTable(
                name: "NewPostCounterAgents");
        }
    }
}
