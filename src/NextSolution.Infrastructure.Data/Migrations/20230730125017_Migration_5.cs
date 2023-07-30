using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextSolution.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migration_5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "User");
        }
    }
}
