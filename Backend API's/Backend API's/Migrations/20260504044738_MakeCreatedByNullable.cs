using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_API_s.Migrations
{
    /// <inheritdoc />
    public partial class MakeCreatedByNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TranslationKeys_Users_CreatedBy",
                table: "TranslationKeys");

            migrationBuilder.DropIndex(
                name: "IX_TranslationKeys_CreatedBy",
                table: "TranslationKeys");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TranslationKeys");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "TranslationKeys",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TranslationKeys_CreatedBy",
                table: "TranslationKeys",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationKeys_Users_CreatedBy",
                table: "TranslationKeys",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
