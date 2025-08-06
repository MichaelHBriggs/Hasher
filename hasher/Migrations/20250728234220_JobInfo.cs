using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hasher.Migrations
{
    /// <inheritdoc />
    public partial class JobInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RootFolder = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Extensions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FoundFilesCount = table.Column<int>(type: "int", nullable: false),
                    ProcessedFilesCount = table.Column<int>(type: "int", nullable: false),
                    MostRecentRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MostRecentFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Files_MostRecentFileId",
                        column: x => x.MostRecentFileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jobs_RunResults_MostRecentRunId",
                        column: x => x.MostRecentRunId,
                        principalTable: "RunResults",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_MostRecentFileId",
                table: "Jobs",
                column: "MostRecentFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_MostRecentRunId",
                table: "Jobs",
                column: "MostRecentRunId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
