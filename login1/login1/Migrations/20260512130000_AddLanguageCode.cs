using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace login1.Migrations
{
    public partial class AddLanguageCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add nullable Code column
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Languages",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            // Populate Code for existing rows
            migrationBuilder.Sql("UPDATE Languages SET Code = 'en' WHERE Id = 1");
            migrationBuilder.Sql("UPDATE Languages SET Code = 'es' WHERE Id = 2");
            migrationBuilder.Sql("UPDATE Languages SET Code = 'fr' WHERE Id = 3");
            migrationBuilder.Sql("UPDATE Languages SET Code = 'de' WHERE Id = 4");
            migrationBuilder.Sql("UPDATE Languages SET Code = 'ja' WHERE Id = 5");
            migrationBuilder.Sql("UPDATE Languages SET Code = 'zh' WHERE Id = 6");

            // Make Code non-nullable now that values are present
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Languages",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Languages");
        }
    }
}
