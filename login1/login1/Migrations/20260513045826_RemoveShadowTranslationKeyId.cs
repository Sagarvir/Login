using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace login1.Migrations
{
    public partial class RemoveShadowTranslationKeyId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
-- If the table still uses KeyId, rename it to TranslationKeyId
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND name = 'TranslationKeyId')
AND EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND name = 'KeyId')
BEGIN
    -- drop FK that references KeyId if present
    DECLARE @fk1 nvarchar(200) = (
        SELECT TOP(1) fk.name
        FROM sys.foreign_keys fk
        JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
        WHERE fk.parent_object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND c.name = 'KeyId'
    );
    IF @fk1 IS NOT NULL
        EXEC('ALTER TABLE [dbo].[TranslationValues] DROP CONSTRAINT [' + @fk1 + ']');

    -- drop index on KeyId if it exists
    IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND name = 'IX_TranslationValues_KeyId_LanguageCode')
        DROP INDEX [IX_TranslationValues_KeyId_LanguageCode] ON [dbo].[TranslationValues];

    -- rename the column
    EXEC sp_rename 'dbo.TranslationValues.KeyId', 'TranslationKeyId', 'COLUMN';
END

-- Now drop any shadow column TranslationKeyId1 if present
IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND name = 'TranslationKeyId1')
BEGIN
    -- drop FK constraint that references the shadow column (if any)
    DECLARE @fkName nvarchar(200) = (
        SELECT TOP(1) fk.name
        FROM sys.foreign_keys fk
        JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
        JOIN sys.columns c ON fkc.parent_object_id = c.object_id AND fkc.parent_column_id = c.column_id
        WHERE fk.parent_object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND c.name = 'TranslationKeyId1'
    );
    IF @fkName IS NOT NULL
        EXEC('ALTER TABLE [dbo].[TranslationValues] DROP CONSTRAINT [' + @fkName + ']');

    -- drop index on the shadow column if present
    IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND name = 'IX_TranslationValues_TranslationKeyId1')
        DROP INDEX [IX_TranslationValues_TranslationKeyId1] ON [dbo].[TranslationValues];

    IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND name = 'IX_TranslationValues_TranslationKeyId1_LanguageCode')
        DROP INDEX [IX_TranslationValues_TranslationKeyId1_LanguageCode] ON [dbo].[TranslationValues];

    -- drop the shadow column
    ALTER TABLE [dbo].[TranslationValues] DROP COLUMN [TranslationKeyId1];
END

-- Ensure unique index on (TranslationKeyId, LanguageCode) exists (only if TranslationKeyId column exists)
IF EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND name = 'TranslationKeyId')
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND name = 'IX_TranslationValues_TranslationKeyId_LanguageCode'
    )
    BEGIN
        CREATE UNIQUE INDEX [IX_TranslationValues_TranslationKeyId_LanguageCode]
        ON [dbo].[TranslationValues]([TranslationKeyId], [LanguageCode]);
    END
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse is non-destructive: recreate shadow column if needed (nullable) and a FK linking it to TranslationKeys
            migrationBuilder.Sql(@"
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TranslationValues]') AND name = 'TranslationKeyId1')
BEGIN
    ALTER TABLE [dbo].[TranslationValues] ADD [TranslationKeyId1] int NULL;
    -- add FK constraint back (if TranslationKeys exists)
    IF EXISTS(SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TranslationKeys]') AND type = 'U')
    BEGIN
        ALTER TABLE [dbo].[TranslationValues] ADD CONSTRAINT FK_TranslationValues_TranslationKeys_TranslationKeyId1 FOREIGN KEY([TranslationKeyId1]) REFERENCES [dbo].[TranslationKeys]([Id]);
    END
END
");
        }
    }
}

