using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSSFeedify.Migrations
{
    /// <inheritdoc />
    public partial class _202410721035 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "RSSFeeds",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RSSFeeds_ApplicationUserId",
                table: "RSSFeeds",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RSSFeeds_AspNetUsers_ApplicationUserId",
                table: "RSSFeeds",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RSSFeeds_AspNetUsers_ApplicationUserId",
                table: "RSSFeeds");

            migrationBuilder.DropIndex(
                name: "IX_RSSFeeds_ApplicationUserId",
                table: "RSSFeeds");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "RSSFeeds");
        }
    }
}
