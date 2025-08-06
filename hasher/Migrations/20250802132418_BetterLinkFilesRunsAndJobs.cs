using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hasher.Migrations
{
    /// <inheritdoc />
    public partial class BetterLinkFilesRunsAndJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Jobs_JobInfoId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "JobInfoId",
                table: "Files",
                newName: "LastJobId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_JobInfoId",
                table: "Files",
                newName: "IX_Files_LastJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_LastRunId",
                table: "Files",
                column: "LastRunId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Jobs_LastJobId",
                table: "Files",
                column: "LastJobId",
                principalTable: "Jobs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_RunResults_LastRunId",
                table: "Files",
                column: "LastRunId",
                principalTable: "RunResults",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Jobs_LastJobId",
                table: "Files");

            migrationBuilder.DropForeignKey(
                name: "FK_Files_RunResults_LastRunId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_LastRunId",
                table: "Files");

            migrationBuilder.RenameColumn(
                name: "LastJobId",
                table: "Files",
                newName: "JobInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_Files_LastJobId",
                table: "Files",
                newName: "IX_Files_JobInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Jobs_JobInfoId",
                table: "Files",
                column: "JobInfoId",
                principalTable: "Jobs",
                principalColumn: "Id");
        }
    }
}
