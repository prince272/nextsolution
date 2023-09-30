using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextSolution.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migration_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MediaType",
                table: "Media",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "FileSize",
                table: "Media",
                newName: "Size");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "Media",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "FileId",
                table: "Media",
                newName: "Path");

            migrationBuilder.RenameIndex(
                name: "IX_Media_FileId",
                table: "Media",
                newName: "IX_Media_Path");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Media",
                newName: "MediaType");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "Media",
                newName: "FileSize");

            migrationBuilder.RenameColumn(
                name: "Path",
                table: "Media",
                newName: "FileId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Media",
                newName: "FileName");

            migrationBuilder.RenameIndex(
                name: "IX_Media_Path",
                table: "Media",
                newName: "IX_Media_FileId");
        }
    }
}
