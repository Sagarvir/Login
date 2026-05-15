using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace login1.Migrations
{
    public partial class SyncLanguageCodeLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make TranslationValues.LanguageCode nvarchar(10) to match Languages.Code
            migrationBuilder.Sql(@"
-- Only change if current length differs
IF EXISTS(
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TranslationValues' AND COLUMN_NAME = 'LanguageCode' AND (CHARACTER_MAXIMUM_LENGTH IS NULL OR CHARACTER_MAXIMUM_LENGTH <> 10)
)
BEGIN
    -- drop composite unique index first if it exists (depends on LanguageCode)
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TranslationValues') AND name = 'IX_TranslationValues_TranslationKeyId_LanguageCode')
        DROP INDEX [IX_TranslationValues_TranslationKeyId_LanguageCode] ON [dbo].[TranslationValues];

    -- drop non-composite index if it exists
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TranslationValues') AND name = 'IX_TranslationValues_LanguageCode')
        DROP INDEX [IX_TranslationValues_LanguageCode] ON [dbo].[TranslationValues];

    -- drop default constraint on column if any
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TranslationValues]') AND [c].[name] = N'LanguageCode');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [TranslationValues] DROP CONSTRAINT [' + @var0 + ']');

    ALTER TABLE [dbo].[TranslationValues] ALTER COLUMN [LanguageCode] NVARCHAR(10) NOT NULL;

    -- recreate non-composite index used by EF if not exists
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TranslationValues') AND name = 'IX_TranslationValues_LanguageCode')
        CREATE INDEX [IX_TranslationValues_LanguageCode] ON [dbo].[TranslationValues]([LanguageCode]);

    -- recreate composite unique index if not exists
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TranslationValues') AND name = 'IX_TranslationValues_TranslationKeyId_LanguageCode')
        CREATE UNIQUE INDEX [IX_TranslationValues_TranslationKeyId_LanguageCode] ON [dbo].[TranslationValues]([TranslationKeyId], [LanguageCode]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- Revert to nvarchar(450) if needed
IF EXISTS(
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'TranslationValues' AND COLUMN_NAME = 'LanguageCode' AND (CHARACTER_MAXIMUM_LENGTH IS NULL OR CHARACTER_MAXIMUM_LENGTH <> 450)
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TranslationValues') AND name = 'IX_TranslationValues_TranslationKeyId_LanguageCode')
        DROP INDEX [IX_TranslationValues_TranslationKeyId_LanguageCode] ON [dbo].[TranslationValues];

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TranslationValues') AND name = 'IX_TranslationValues_LanguageCode')
        DROP INDEX [IX_TranslationValues_LanguageCode] ON [dbo].[TranslationValues];

    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TranslationValues]') AND [c].[name] = N'LanguageCode');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [TranslationValues] DROP CONSTRAINT [' + @var0 + ']');

    ALTER TABLE [dbo].[TranslationValues] ALTER COLUMN [LanguageCode] NVARCHAR(450) NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TranslationValues') AND name = 'IX_TranslationValues_LanguageCode')
        CREATE INDEX [IX_TranslationValues_LanguageCode] ON [dbo].[TranslationValues]([LanguageCode]);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.TranslationValues') AND name = 'IX_TranslationValues_TranslationKeyId_LanguageCode')
        CREATE UNIQUE INDEX [IX_TranslationValues_TranslationKeyId_LanguageCode] ON [dbo].[TranslationValues]([TranslationKeyId], [LanguageCode]);
END
");
        }
    }
}
