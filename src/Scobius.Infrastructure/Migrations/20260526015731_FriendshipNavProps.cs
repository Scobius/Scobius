using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scobius.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FriendshipNavProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_friendRequests_AspNetUsers_ReceiverId",
                table: "friendRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_friendRequests_AspNetUsers_SenderId",
                table: "friendRequests");

            migrationBuilder.CreateIndex(
                name: "IX_friendships_User1Id",
                table: "friendships",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_friendships_User2Id",
                table: "friendships",
                column: "User2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_friendRequests_AspNetUsers_ReceiverId",
                table: "friendRequests",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_friendRequests_AspNetUsers_SenderId",
                table: "friendRequests",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_friendships_AspNetUsers_User1Id",
                table: "friendships",
                column: "User1Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_friendships_AspNetUsers_User2Id",
                table: "friendships",
                column: "User2Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_friendRequests_AspNetUsers_ReceiverId",
                table: "friendRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_friendRequests_AspNetUsers_SenderId",
                table: "friendRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_friendships_AspNetUsers_User1Id",
                table: "friendships");

            migrationBuilder.DropForeignKey(
                name: "FK_friendships_AspNetUsers_User2Id",
                table: "friendships");

            migrationBuilder.DropIndex(
                name: "IX_friendships_User1Id",
                table: "friendships");

            migrationBuilder.DropIndex(
                name: "IX_friendships_User2Id",
                table: "friendships");

            migrationBuilder.AddForeignKey(
                name: "FK_friendRequests_AspNetUsers_ReceiverId",
                table: "friendRequests",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_friendRequests_AspNetUsers_SenderId",
                table: "friendRequests",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
