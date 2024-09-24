using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BroadcastSocialMedia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePathToBroadcast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
            name: "ImagePath",
            table: "Broadcasts",
            type: "nvarchar(max)",
            nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Broadcasts");
        }
    }
}
