using Microsoft.EntityFrameworkCore.Migrations;

namespace API.Data.Migrations
{
    public partial class ChangedToRecipient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecipentDeleted",
                table: "Messages",
                newName: "RecipientDeleted");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecipientDeleted",
                table: "Messages",
                newName: "RecipentDeleted");
        }
    }
}
