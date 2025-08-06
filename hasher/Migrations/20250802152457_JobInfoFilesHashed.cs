using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hasher.Migrations
{
    /// <inheritdoc />
    public partial class JobInfoFilesHashed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FilesHashedCount",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilesHashedCount",
                table: "Jobs");
        }
    }
}
