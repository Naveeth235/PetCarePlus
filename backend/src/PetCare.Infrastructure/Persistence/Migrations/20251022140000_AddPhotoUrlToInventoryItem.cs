using Microsoft.EntityFrameworkCore.Migrations;

namespace PetCare.Infrastructure.Persistence.Migrations
{
    public partial class AddPhotoUrlToInventoryItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "InventoryItems",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "InventoryItems");
        }
    }
}
