using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BroadcastSocialMedia.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContentColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Broadcasts_AspNetUsers_UserId",
                table: "Broadcasts");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Broadcasts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Broadcasts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Broadcasts_AspNetUsers_UserId",
                table: "Broadcasts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Broadcasts_AspNetUsers_UserId",
                table: "Broadcasts");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Broadcasts");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Broadcasts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Broadcasts_AspNetUsers_UserId",
                table: "Broadcasts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
