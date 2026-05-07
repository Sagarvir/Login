using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend_API_s.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTranslationAddedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Translations_Users_AddedBy",
                table: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Translations_AddedBy",
                table: "Translations");

            migrationBuilder.DropColumn(
                name: "AddedBy",
                table: "Translations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AddedBy",
                table: "Translations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Translations_AddedBy",
                table: "Translations",
                column: "AddedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Translations_Users_AddedBy",
                table: "Translations",
                column: "AddedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
