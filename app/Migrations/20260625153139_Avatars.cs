using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessengerApp.Migrations
{
    /// <inheritdoc />
    public partial class Avatars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageData",
                table: "Chats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarData",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "AvatarData",
                table: "AspNetUsers");
        }
    }
}
