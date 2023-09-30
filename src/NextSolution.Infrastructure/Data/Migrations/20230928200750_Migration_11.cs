using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextSolution.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migration_11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                table: "ChatMessage",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_ParentId",
                table: "ChatMessage",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessage_ChatMessage_ParentId",
                table: "ChatMessage",
                column: "ParentId",
                principalTable: "ChatMessage",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessage_ChatMessage_ParentId",
                table: "ChatMessage");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessage_ParentId",
                table: "ChatMessage");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "ChatMessage");
        }
    }
}
