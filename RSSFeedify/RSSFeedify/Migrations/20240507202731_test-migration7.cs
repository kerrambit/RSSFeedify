using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSSFeedify.Migrations
{
    /// <inheritdoc />
    public partial class testmigration7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RSSFeedsItems_RSSFeeds_RSSFeedId",
                table: "RSSFeedsItems");

            migrationBuilder.DropIndex(
                name: "IX_RSSFeedsItems_RSSFeedId",
                table: "RSSFeedsItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RSSFeedsItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RSSFeedsItems",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RSSFeeds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RSSFeeds",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RSSFeedsItems");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RSSFeedsItems");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RSSFeeds");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RSSFeeds");

            migrationBuilder.CreateIndex(
                name: "IX_RSSFeedsItems_RSSFeedId",
                table: "RSSFeedsItems",
                column: "RSSFeedId");

            migrationBuilder.AddForeignKey(
                name: "FK_RSSFeedsItems_RSSFeeds_RSSFeedId",
                table: "RSSFeedsItems",
                column: "RSSFeedId",
                principalTable: "RSSFeeds",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
