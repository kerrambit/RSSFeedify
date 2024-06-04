using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSSFeedify.Migrations
{
    /// <inheritdoc />
    public partial class testmigration5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RSSFeedsItems",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Links = table.Column<string[]>(type: "text[]", nullable: false),
                    Categories = table.Column<List<string>>(type: "text[]", nullable: false),
                    Authors = table.Column<List<string>>(type: "text[]", nullable: false),
                    Contributors = table.Column<List<string>>(type: "text[]", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RSSFeedsItems", x => x.Guid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RSSFeedsItems");
        }
    }
}
