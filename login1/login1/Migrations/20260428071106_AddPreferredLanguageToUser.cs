using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace login1.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredLanguageToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredLanguage",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "PreferredLanguageId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PreferredLanguageId",
                table: "Users",
                column: "PreferredLanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Languages_PreferredLanguageId",
                table: "Users",
                column: "PreferredLanguageId",
                principalTable: "Languages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Languages_PreferredLanguageId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PreferredLanguageId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredLanguageId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "PreferredLanguage",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
