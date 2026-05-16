using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace login1.Migrations
{
    /// <inheritdoc />
    public partial class CleanFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    -- Drop FK if exists
    IF EXISTS (
        SELECT 1 FROM sys.foreign_keys 
        WHERE name = 'FK_TranslationValues_TranslationKeys_TranslationKeyId1'
    )
    BEGIN
        ALTER TABLE [TranslationValues] 
        DROP CONSTRAINT [FK_TranslationValues_TranslationKeys_TranslationKeyId1];
    END

    -- Drop index if exists
    IF EXISTS (
        SELECT 1 FROM sys.indexes 
        WHERE name = 'IX_TranslationValues_TranslationKeyId1'
    )
    BEGIN
        DROP INDEX [IX_TranslationValues_TranslationKeyId1] 
        ON [TranslationValues];
    END

    -- Drop column if exists
    IF EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE Name = N'TranslationKeyId1' 
        AND Object_ID = Object_ID(N'TranslationValues')
    )
    BEGIN
        ALTER TABLE [TranslationValues] 
        DROP COLUMN [TranslationKeyId1];
    END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TranslationKeyId1",
                table: "TranslationValues",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TranslationValues_TranslationKeyId1",
                table: "TranslationValues",
                column: "TranslationKeyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TranslationValues_TranslationKeys_TranslationKeyId1",
                table: "TranslationValues",
                column: "TranslationKeyId1",
                principalTable: "TranslationKeys",
                principalColumn: "Id");
        }
    }
}
