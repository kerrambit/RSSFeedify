using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSSFeedify.Migrations
{
    /// <inheritdoc />
    public partial class testmigration6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RSSFeedId",
                table: "RSSFeedsItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RSSFeedsItems_RSSFeeds_RSSFeedId",
                table: "RSSFeedsItems");

            migrationBuilder.DropIndex(
                name: "IX_RSSFeedsItems_RSSFeedId",
                table: "RSSFeedsItems");

            migrationBuilder.DropColumn(
                name: "RSSFeedId",
                table: "RSSFeedsItems");
        }
    }
}
