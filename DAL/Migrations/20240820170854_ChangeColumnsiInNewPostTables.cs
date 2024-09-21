using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HM.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColumnsiInNewPostTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewPostContactPersons_NewPostCounterAgents_CounterpartyRef",
                table: "NewPostContactPersons");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "NewPostContactPersons");

            migrationBuilder.RenameColumn(
                name: "OwnershipFormId",
                table: "NewPostCounterAgents",
                newName: "OwnershipForm");

            migrationBuilder.RenameColumn(
                name: "CounterpartyId",
                table: "NewPostCounterAgents",
                newName: "Counterparty");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "NewPostContactPersons",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "CounterpartyRef",
                table: "NewPostContactPersons",
                newName: "CounterAgentId");

            migrationBuilder.RenameIndex(
                name: "IX_NewPostContactPersons_CounterpartyRef",
                table: "NewPostContactPersons",
                newName: "IX_NewPostContactPersons_CounterAgentId");

            migrationBuilder.AddColumn<string>(
                name: "Ref",
                table: "NewPostContactPersons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_NewPostContactPersons_NewPostCounterAgents_CounterAgentId",
                table: "NewPostContactPersons",
                column: "CounterAgentId",
                principalTable: "NewPostCounterAgents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewPostContactPersons_NewPostCounterAgents_CounterAgentId",
                table: "NewPostContactPersons");

            migrationBuilder.DropColumn(
                name: "Ref",
                table: "NewPostContactPersons");

            migrationBuilder.RenameColumn(
                name: "OwnershipForm",
                table: "NewPostCounterAgents",
                newName: "OwnershipFormId");

            migrationBuilder.RenameColumn(
                name: "Counterparty",
                table: "NewPostCounterAgents",
                newName: "CounterpartyId");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "NewPostContactPersons",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "CounterAgentId",
                table: "NewPostContactPersons",
                newName: "CounterpartyRef");

            migrationBuilder.RenameIndex(
                name: "IX_NewPostContactPersons_CounterAgentId",
                table: "NewPostContactPersons",
                newName: "IX_NewPostContactPersons_CounterpartyRef");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "NewPostContactPersons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NewPostContactPersons_NewPostCounterAgents_CounterpartyRef",
                table: "NewPostContactPersons",
                column: "CounterpartyRef",
                principalTable: "NewPostCounterAgents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
