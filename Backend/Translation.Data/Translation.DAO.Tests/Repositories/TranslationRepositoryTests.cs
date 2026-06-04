using Translation.Contracts.DTO.Translation;
using Translation.DAO.Data;
using Translation.DAO.Repositories;
using Translation.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Translation.DAO.Tests.Repositories
{
    [TestClass]
    public class TranslationRepositoryTests
    {
        private AppDbContext _context = null!;
        private TranslationRepository _sut = null!;

        // ── Seed helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Seeds a Language and a TranslationKey and returns both.
        /// TranslationValue FK requires a Language with matching Code to exist first.
        /// </summary>
        private async Task<(Language lang, TranslationKey key)> SeedKeyAndLanguageAsync(
            string langCode = "EN",
            string keyName = "WELCOME",
            int projectId = 1)
        {
            var lang = new Language { Code = langCode, Name = "English" };
            _context.Languages.Add(lang);

            var key = new TranslationKey
            {
                KeyName = keyName,
                OriginalText = "Welcome",
                ProjectId = projectId,
                CreatedByEmpId = "emp001",
                IsActive = true
            };
            _context.TranslationKeys.Add(key);
            await _context.SaveChangesAsync();
            return (lang, key);
        }

        [TestInitialize]
        public void SetUp()
        {
            _context = TestDbContextFactory.Create();
            _sut = new TranslationRepository(_context);
        }

        [TestCleanup]
        public void TearDown()
        {
            _context.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // KeyExists
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task KeyExists_ExistingActiveKey_ReturnsTrue()
        {
            await SeedKeyAndLanguageAsync(keyName: "EXISTING_KEY", projectId: 1);

            var result = await _sut.KeyExists("EXISTING_KEY", 1);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task KeyExists_NonExistingKey_ReturnsFalse()
        {
            var result = await _sut.KeyExists("GHOST_KEY", 1);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task KeyExists_InactiveKey_ReturnsFalse()
        {
            var lang = new Language { Code = "EN", Name = "English" };
            _context.Languages.Add(lang);
            _context.TranslationKeys.Add(new TranslationKey
            {
                KeyName = "INACTIVE_KEY",
                OriginalText = "text",
                ProjectId = 1,
                CreatedByEmpId = "emp001",
                IsActive = false    // inactive
            });
            await _context.SaveChangesAsync();

            var result = await _sut.KeyExists("INACTIVE_KEY", 1);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task KeyExists_SameKeyNameDifferentProject_ReturnsFalse()
        {
            await SeedKeyAndLanguageAsync(keyName: "MY_KEY", projectId: 1);

            // Look for same key name but in project 2
            var result = await _sut.KeyExists("MY_KEY", 2);

            Assert.IsFalse(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // AddKey
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task AddKey_ValidKey_PersistsToDatabase()
        {
            var key = new TranslationKey
            {
                KeyName = "NEW_KEY",
                OriginalText = "New Key",
                ProjectId = 1,
                CreatedByEmpId = "emp001",
                IsActive = true
            };

            await _sut.AddKey(key);

            var saved = await _context.TranslationKeys.FindAsync(key.Id);
            Assert.IsNotNull(saved);
            Assert.AreEqual("NEW_KEY", saved.KeyName);
        }

        [TestMethod]
        public async Task AddKey_AssignsIdAfterSave()
        {
            var key = new TranslationKey
            {
                KeyName = "KEY_ID_TEST",
                OriginalText = "text",
                ProjectId = 1,
                CreatedByEmpId = "emp001",
                IsActive = true
            };

            await _sut.AddKey(key);

            Assert.IsTrue(key.Id > 0);
        }

        // ═════════════════════════════════════════════════════════════════════
        // AddKeys (bulk)
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task AddKeys_MultipleKeys_AllPersist()
        {
            var keys = new List<TranslationKey>
            {
                new() { KeyName = "KEY_A", OriginalText = "A", ProjectId = 1, CreatedByEmpId = "emp001", IsActive = true },
                new() { KeyName = "KEY_B", OriginalText = "B", ProjectId = 1, CreatedByEmpId = "emp001", IsActive = true },
                new() { KeyName = "KEY_C", OriginalText = "C", ProjectId = 1, CreatedByEmpId = "emp001", IsActive = true }
            };

            await _sut.AddKeys(keys);

            var saved = await _context.TranslationKeys.ToListAsync();
            Assert.AreEqual(3, saved.Count);
        }

        [TestMethod]
        public async Task AddKeys_EmptyList_SavesNothingAndDoesNotThrow()
        {
            await _sut.AddKeys(new List<TranslationKey>());

            var count = await _context.TranslationKeys.CountAsync();
            Assert.AreEqual(0, count);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetAllKeys
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetAllKeys_OnlyReturnsActiveKeys()
        {
            _context.TranslationKeys.AddRange(
                new TranslationKey { KeyName = "ACTIVE_KEY", OriginalText = "a", ProjectId = 1, CreatedByEmpId = "e", IsActive = true },
                new TranslationKey { KeyName = "INACTIVE_KEY", OriginalText = "b", ProjectId = 1, CreatedByEmpId = "e", IsActive = false }
            );
            await _context.SaveChangesAsync();

            var result = await _sut.GetAllKeys();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("ACTIVE_KEY", result[0].KeyName);
        }

        [TestMethod]
        public async Task GetAllKeys_EmptyDatabase_ReturnsEmptyList()
        {
            var result = await _sut.GetAllKeys();

            Assert.AreEqual(0, result.Count);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetExistingKeys
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetExistingKeys_MatchingKeys_ReturnsThoseKeys()
        {
            _context.TranslationKeys.Add(new TranslationKey
            {
                KeyName = "KEY_ONE",
                OriginalText = "One",
                ProjectId = 1,
                CreatedByEmpId = "emp001",
                IsActive = true
            });
            await _context.SaveChangesAsync();

            var input = new List<CreateKeyItem>
            {
                new() { KeyName = "KEY_ONE", ProjectId = 1 }
            };

            var result = await _sut.GetExistingKeys(input);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("KEY_ONE", result[0].KeyName);
        }

        [TestMethod]
        public async Task GetExistingKeys_NoMatchingKeys_ReturnsEmptyList()
        {
            var input = new List<CreateKeyItem>
            {
                new() { KeyName = "GHOST_KEY", ProjectId = 99 }
            };

            var result = await _sut.GetExistingKeys(input);

            Assert.AreEqual(0, result.Count);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetValidKeyIdsAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetValidKeyIds_AllActiveIds_ReturnsAll()
        {
            var (_, key) = await SeedKeyAndLanguageAsync(keyName: "KEY_VALID");

            var result = await _sut.GetValidKeyIdsAsync(new List<int> { key.Id });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(key.Id, result[0]);
        }

        [TestMethod]
        public async Task GetValidKeyIds_InactiveKey_NotReturned()
        {
            var key = new TranslationKey
            {
                KeyName = "INACTIVE",
                OriginalText = "x",
                ProjectId = 1,
                CreatedByEmpId = "e",
                IsActive = false
            };
            _context.TranslationKeys.Add(key);
            await _context.SaveChangesAsync();

            var result = await _sut.GetValidKeyIdsAsync(new List<int> { key.Id });

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetValidKeyIds_MixedIds_ReturnsOnlyValid()
        {
            var (_, key) = await SeedKeyAndLanguageAsync(keyName: "VALID_KEY");

            var result = await _sut.GetValidKeyIdsAsync(new List<int> { key.Id, 9999 });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(key.Id, result[0]);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetKeyIdByNameAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetKeyIdByName_ExistingActiveKey_ReturnsId()
        {
            var (_, key) = await SeedKeyAndLanguageAsync(keyName: "FIND_ME");

            var result = await _sut.GetKeyIdByNameAsync("FIND_ME");

            Assert.IsNotNull(result);
            Assert.AreEqual(key.Id, result);
        }

        [TestMethod]
        public async Task GetKeyIdByName_NonExistingKey_ReturnsNull()
        {
            var result = await _sut.GetKeyIdByNameAsync("GHOST");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetKeyIdByName_InactiveKey_ReturnsNull()
        {
            _context.TranslationKeys.Add(new TranslationKey
            {
                KeyName = "INACTIVE_FIND",
                OriginalText = "x",
                ProjectId = 1,
                CreatedByEmpId = "e",
                IsActive = false
            });
            await _context.SaveChangesAsync();

            var result = await _sut.GetKeyIdByNameAsync("INACTIVE_FIND");

            Assert.IsNull(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // TranslationKeyExistsAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task TranslationKeyExists_ActiveKey_ReturnsTrue()
        {
            var (_, key) = await SeedKeyAndLanguageAsync(keyName: "EXISTS_KEY");

            var result = await _sut.TranslationKeyExistsAsync(key.Id);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TranslationKeyExists_NonExistingId_ReturnsFalse()
        {
            var result = await _sut.TranslationKeyExistsAsync(9999);

            Assert.IsFalse(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // LanguageExistsAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task LanguageExists_ExistingCode_ReturnsTrue()
        {
            _context.Languages.Add(new Language { Code = "EN", Name = "English" });
            await _context.SaveChangesAsync();

            var result = await _sut.LanguageExistsAsync("EN");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task LanguageExists_NonExistingCode_ReturnsFalse()
        {
            var result = await _sut.LanguageExistsAsync("XX");

            Assert.IsFalse(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // SaveTranslationAsync & GetTranslationValueAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task SaveTranslation_ValidTranslation_PersistsToDatabase()
        {
            var (lang, key) = await SeedKeyAndLanguageAsync();

            var translation = new TranslationValue
            {
                TranslationKeyId = key.Id,
                LanguageCode = lang.Code,
                Value = "Hello",
                UpdatedByEmpId = "emp001"
            };

            await _sut.SaveTranslationAsync(translation);

            var saved = await _context.TranslationValues.FindAsync(translation.Id);
            Assert.IsNotNull(saved);
            Assert.AreEqual("Hello", saved.Value);
        }

        [TestMethod]
        public async Task GetTranslationValue_ExistingTranslation_ReturnsIt()
        {
            var (lang, key) = await SeedKeyAndLanguageAsync();
            _context.TranslationValues.Add(new TranslationValue
            {
                TranslationKeyId = key.Id,
                LanguageCode = lang.Code,
                Value = "Hello"
            });
            await _context.SaveChangesAsync();

            var result = await _sut.GetTranslationValueAsync(key.Id, lang.Code);

            Assert.IsNotNull(result);
            Assert.AreEqual("Hello", result.Value);
        }

        [TestMethod]
        public async Task GetTranslationValue_NonExisting_ReturnsNull()
        {
            var result = await _sut.GetTranslationValueAsync(999, "EN");

            Assert.IsNull(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetTranslationForUiAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetTranslationForUi_ExistingTranslation_ReturnsProjectedResult()
        {
            var (lang, key) = await SeedKeyAndLanguageAsync();
            _context.TranslationValues.Add(new TranslationValue
            {
                TranslationKeyId = key.Id,
                LanguageCode = lang.Code,
                Value = "Welcome"
            });
            await _context.SaveChangesAsync();

            var result = await _sut.GetTranslationForUiAsync(key.Id, lang.Code);

            Assert.IsNotNull(result);
            Assert.AreEqual("Welcome", result.Value);
            Assert.AreEqual(lang.Code, result.LanguageCode);
        }

        [TestMethod]
        public async Task GetTranslationForUi_NotFound_ReturnsNull()
        {
            var result = await _sut.GetTranslationForUiAsync(999, "EN");

            Assert.IsNull(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetTranslationsByKeyAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetTranslationsByKey_MultipleLanguages_ReturnsAll()
        {
            var lang1 = new Language { Code = "EN", Name = "English" };
            var lang2 = new Language { Code = "FR", Name = "French" };
            _context.Languages.AddRange(lang1, lang2);

            var key = new TranslationKey
            {
                KeyName = "MULTI_LANG",
                OriginalText = "text",
                ProjectId = 1,
                CreatedByEmpId = "e",
                IsActive = true
            };
            _context.TranslationKeys.Add(key);
            await _context.SaveChangesAsync();

            _context.TranslationValues.AddRange(
                new TranslationValue { TranslationKeyId = key.Id, LanguageCode = "EN", Value = "Hello" },
                new TranslationValue { TranslationKeyId = key.Id, LanguageCode = "FR", Value = "Bonjour" }
            );
            await _context.SaveChangesAsync();

            var result = await _sut.GetTranslationsByKeyAsync(key.Id);

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task GetTranslationsByKey_NoTranslations_ReturnsEmptyList()
        {
            var (_, key) = await SeedKeyAndLanguageAsync(keyName: "EMPTY_KEY");

            var result = await _sut.GetTranslationsByKeyAsync(key.Id);

            Assert.AreEqual(0, result.Count);
        }

        // ═════════════════════════════════════════════════════════════════════
        // InsertBulkAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task InsertBulk_NewTranslations_AllInserted()
        {
            var (lang, key) = await SeedKeyAndLanguageAsync();

            var items = new List<BulkTranslationItem>
            {
                new() { KeyId = key.Id, LanguageCode = lang.Code, Value = "Hello" }
            };

            await _sut.InsertBulkAsync(items, new List<TranslationValue>(), "emp001");

            var saved = await _context.TranslationValues.ToListAsync();
            Assert.AreEqual(1, saved.Count);
            Assert.AreEqual("Hello", saved[0].Value);
        }

        [TestMethod]
        public async Task InsertBulk_ExistingTranslation_SkipsUpdate()
        {
            // InsertBulkAsync currently skips updates (update logic is commented out)
            var (lang, key) = await SeedKeyAndLanguageAsync();

            var existing = new TranslationValue
            {
                TranslationKeyId = key.Id,
                LanguageCode = lang.Code,
                Value = "OldValue"
            };
            _context.TranslationValues.Add(existing);
            await _context.SaveChangesAsync();

            var items = new List<BulkTranslationItem>
            {
                new() { KeyId = key.Id, LanguageCode = lang.Code, Value = "NewValue" }
            };

            await _sut.InsertBulkAsync(items, new List<TranslationValue> { existing }, "emp001");

            var saved = await _context.TranslationValues.FindAsync(existing.Id);
            // Value should remain unchanged because update is skipped
            Assert.AreEqual("OldValue", saved!.Value);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetKeyByIdAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetKeyById_ActiveKey_ReturnsKeyWithTranslations()
        {
            var (lang, key) = await SeedKeyAndLanguageAsync();
            _context.TranslationValues.Add(new TranslationValue
            {
                TranslationKeyId = key.Id,
                LanguageCode = lang.Code,
                Value = "Hello"
            });
            await _context.SaveChangesAsync();

            var result = await _sut.GetKeyByIdAsync(key.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Translations.Count);
        }

        [TestMethod]
        public async Task GetKeyById_NonExistingId_ReturnsNull()
        {
            var result = await _sut.GetKeyByIdAsync(9999);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetKeyById_InactiveKey_ReturnsNull()
        {
            var key = new TranslationKey
            {
                KeyName = "INACTIVE_GET",
                OriginalText = "x",
                ProjectId = 1,
                CreatedByEmpId = "e",
                IsActive = false
            };
            _context.TranslationKeys.Add(key);
            await _context.SaveChangesAsync();

            var result = await _sut.GetKeyByIdAsync(key.Id);

            Assert.IsNull(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // DeleteValuesAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task DeleteValues_ExistingValues_RemovesFromDatabase()
        {
            var (lang, key) = await SeedKeyAndLanguageAsync();
            var tv = new TranslationValue
            {
                TranslationKeyId = key.Id,
                LanguageCode = lang.Code,
                Value = "Hello"
            };
            _context.TranslationValues.Add(tv);
            await _context.SaveChangesAsync();

            await _sut.DeleteValuesAsync(new List<TranslationValue> { tv });

            var remaining = await _context.TranslationValues.ToListAsync();
            Assert.AreEqual(0, remaining.Count);
        }

        [TestMethod]
        public async Task DeleteValues_EmptyList_DoesNotThrow()
        {
            await _sut.DeleteValuesAsync(new List<TranslationValue>());

            // No exception — test passes
        }

        // ═════════════════════════════════════════════════════════════════════
        // DeleteKeyAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task DeleteKey_ExistingKey_RemovesFromDatabase()
        {
            var (_, key) = await SeedKeyAndLanguageAsync(keyName: "DELETE_ME");

            await _sut.DeleteKeyAsync(key);

            var deleted = await _context.TranslationKeys.FindAsync(key.Id);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task DeleteKey_OnlyDeletesTargetKey()
        {
            _context.TranslationKeys.AddRange(
                new TranslationKey { KeyName = "DELETE_THIS", OriginalText = "x", ProjectId = 1, CreatedByEmpId = "e", IsActive = true },
                new TranslationKey { KeyName = "KEEP_THIS", OriginalText = "y", ProjectId = 1, CreatedByEmpId = "e", IsActive = true }
            );
            await _context.SaveChangesAsync();

            var toDelete = await _context.TranslationKeys.FirstAsync(k => k.KeyName == "DELETE_THIS");
            await _sut.DeleteKeyAsync(toDelete);

            var remaining = await _context.TranslationKeys.ToListAsync();
            Assert.AreEqual(1, remaining.Count);
            Assert.AreEqual("KEEP_THIS", remaining[0].KeyName);
        }

        // ═════════════════════════════════════════════════════════════════════
        // SavePublishRecordAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task SavePublishRecord_ValidRecord_PersistsToDatabase()
        {
            var record = new TranslationPublish
            {
                Version = "v20250101120000",
                PublishedAt = DateTime.UtcNow,
                PublishedBy = "Creator",
                FileCount = 4
            };

            await _sut.SavePublishRecordAsync(record);

            var saved = await _context.TranslationPublishes.FindAsync(record.Id);
            Assert.IsNotNull(saved);
            Assert.AreEqual("v20250101120000", saved.Version);
            Assert.AreEqual(4, saved.FileCount);
        }
    }
}