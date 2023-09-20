using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NextSolution.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migration_8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumberFirst",
                table: "User",
                newName: "PhoneNumberRequired");

            migrationBuilder.RenameColumn(
                name: "EmailFirst",
                table: "User",
                newName: "EmailRequired");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumberRequired",
                table: "User",
                newName: "PhoneNumberFirst");

            migrationBuilder.RenameColumn(
                name: "EmailRequired",
                table: "User",
                newName: "EmailFirst");
        }
    }
}
