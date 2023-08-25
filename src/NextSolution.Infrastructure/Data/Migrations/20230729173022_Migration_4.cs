using System;
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
            migrationBuilder.CreateTable(
                name: "UserSession",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AccessTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessTokenExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshTokenExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSession_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSession_UserId",
                table: "UserSession",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSession");
        }
    }
}
