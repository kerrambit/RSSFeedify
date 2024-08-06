using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSSFeedify.Migrations
{
    /// <inheritdoc />
    public partial class postgresql_migration_358 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserGuid",
                table: "RSSFeeds",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Nickname = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Guid);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RSSFeeds_UserGuid",
                table: "RSSFeeds",
                column: "UserGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_RSSFeeds_Users_UserGuid",
                table: "RSSFeeds",
                column: "UserGuid",
                principalTable: "Users",
                principalColumn: "Guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RSSFeeds_Users_UserGuid",
                table: "RSSFeeds");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_RSSFeeds_UserGuid",
                table: "RSSFeeds");

            migrationBuilder.DropColumn(
                name: "UserGuid",
                table: "RSSFeeds");
        }
    }
}
