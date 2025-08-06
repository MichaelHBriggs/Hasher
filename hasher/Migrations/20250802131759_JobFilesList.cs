using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace hasher.Migrations
{
    /// <inheritdoc />
    public partial class JobFilesList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Files_MostRecentFileId",
                table: "Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Jobs_MostRecentFileId",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "MostRecentFileId",
                table: "Jobs");

            migrationBuilder.AddColumn<Guid>(
                name: "JobInfoId",
                table: "Files",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_JobInfoId",
                table: "Files",
                column: "JobInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Jobs_JobInfoId",
                table: "Files",
                column: "JobInfoId",
                principalTable: "Jobs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Jobs_JobInfoId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_JobInfoId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "JobInfoId",
                table: "Files");

            migrationBuilder.AddColumn<Guid>(
                name: "MostRecentFileId",
                table: "Jobs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_MostRecentFileId",
                table: "Jobs",
                column: "MostRecentFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Files_MostRecentFileId",
                table: "Jobs",
                column: "MostRecentFileId",
                principalTable: "Files",
                principalColumn: "Id");
        }
    }
}
