using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace login1.Migrations
{
    public partial class AddLanguageFk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- Ensure Languages.Code has a unique index
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Languages') AND name = 'IX_Languages_Code')
BEGIN
    CREATE UNIQUE INDEX [IX_Languages_Code] ON [dbo].[Languages]([Code]);
END

-- Only add FK if it does not exist
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'dbo.TranslationValues') AND name = 'FK_TranslationValues_Languages_LanguageCode'
)
BEGIN
    ALTER TABLE [dbo].[TranslationValues]
    ADD CONSTRAINT [FK_TranslationValues_Languages_LanguageCode] FOREIGN KEY([LanguageCode]) REFERENCES [dbo].[Languages]([Code]) ON DELETE NO ACTION;
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'dbo.TranslationValues') AND name = 'FK_TranslationValues_Languages_LanguageCode'
)
BEGIN
    ALTER TABLE [dbo].[TranslationValues] DROP CONSTRAINT [FK_TranslationValues_Languages_LanguageCode];
END
");
        }
    }
}
