using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentFood.WebAdmin.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsernameToEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "Username");
        }
    }
}
