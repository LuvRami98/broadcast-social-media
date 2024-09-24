using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BroadcastSocialMedia.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContentFromBroadcast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Broadcasts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Broadcasts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
