using Microsoft.AspNetCore.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Translation.Contracts.DTO.Translation;
using Translation.DAO.Repositories.Interfaces;
using Translation.Models.Entities;
using Translation.Service.Services;
using TranslationUser = Translation.Models.Entities.User;

namespace Translation.Tests.Services
{
    [TestClass]
    public class TranslationServiceTests
    {
        // ── Dependencies ──────────────────────────────────────────────────────
        private Mock<ITranslationRepository> _repoMock = null!;
        private Mock<IWebHostEnvironment> _envMock = null!;
        private TranslationService _sut = null!;   // system under test

        // ── Shared test data ──────────────────────────────────────────────────
        private const string EmpId = "emp001";
        private const string LangCode = "EN";
        private const int ValidKeyId = 1;
        private const string ValidKeyName = "WELCOME_MESSAGE";
        private const int ValidProjectId = 10;

        [TestInitialize]
        public void SetUp()
        {
            _repoMock = new Mock<ITranslationRepository>();
            _envMock = new Mock<IWebHostEnvironment>();
            _sut = new TranslationService(_repoMock.Object, _envMock.Object);
        }

        // Helper method for async exception testing
        private async Task AssertThrowsExceptionAsync(Func<Task> action)
        {
            try
            {
                await action();
                Assert.Fail("Expected exception was not thrown");
            }
            catch (Exception)
            {
                // Expected exception was thrown
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        // CreateKey
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task CreateKey_ValidRequest_ReturnsSuccessMessage()
        {
            // Arrange
            var request = new CreateKeyRequest
            {
                KeyName = "welcome_message",
                OriginalText = "Welcome!",
                ProjectId = ValidProjectId
            };

            _repoMock.Setup(r => r.KeyExists(It.IsAny<string>(), ValidProjectId))
                     .ReturnsAsync(false);

            _repoMock.Setup(r => r.AddKey(It.IsAny<TranslationKey>()))
                     .Returns(Task.CompletedTask);

            // Act
            var result = _sut.CreateKey(request, EmpId);
            await result;

            // Assert
            _repoMock.Verify(r => r.AddKey(It.Is<TranslationKey>(k =>
                k.KeyName == "WELCOME_MESSAGE" &&   // uppercased
                k.OriginalText == "Welcome!" &&
                k.ProjectId == ValidProjectId &&
                k.CreatedByEmpId == EmpId &&
                k.IsActive == true
            )), Times.Once);
        }

        [TestMethod]
        public async Task CreateKey_KeyNameIsNormalisedToUpperCase()
        {
            // Arrange
            var request = new CreateKeyRequest
            {
                KeyName = "  hello_world  ",
                OriginalText = "Hello",
                ProjectId = ValidProjectId
            };

            _repoMock.Setup(r => r.KeyExists("HELLO_WORLD", ValidProjectId))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.AddKey(It.IsAny<TranslationKey>()))
                     .Returns(Task.CompletedTask);

            // Act
            await _sut.CreateKey(request, EmpId);

            // Assert – repository must have received the trimmed, uppercased name
            _repoMock.Verify(r => r.AddKey(It.Is<TranslationKey>(k =>
                k.KeyName == "HELLO_WORLD"
            )), Times.Once);
        }

        [TestMethod]
        public async Task CreateKey_MissingOriginalText_ThrowsException()
        {
            var request = new CreateKeyRequest
            {
                KeyName = "some_key",
                OriginalText = "   ",   // whitespace only
                ProjectId = ValidProjectId
            };

            await AssertThrowsExceptionAsync(
                () => _sut.CreateKey(request, EmpId));
        }

        [TestMethod]
        public async Task CreateKey_InvalidProjectId_ThrowsException()
        {
            var request = new CreateKeyRequest
            {
                KeyName = "some_key",
                OriginalText = "Some text",
                ProjectId = 0          // invalid
            };

            await AssertThrowsExceptionAsync(
                () => _sut.CreateKey(request, EmpId));
        }

        [TestMethod]
        public async Task CreateKey_DuplicateKey_ThrowsException()
        {
            var request = new CreateKeyRequest
            {
                KeyName = "duplicate_key",
                OriginalText = "Some text",
                ProjectId = ValidProjectId
            };

            _repoMock.Setup(r => r.KeyExists("DUPLICATE_KEY", ValidProjectId))
                     .ReturnsAsync(true);  // already exists

            await AssertThrowsExceptionAsync(
                () => _sut.CreateKey(request, EmpId));

            // Repo's AddKey must never be called
            _repoMock.Verify(r => r.AddKey(It.IsAny<TranslationKey>()), Times.Never);
        }

        // ═════════════════════════════════════════════════════════════════════
        // CreateKeys  (bulk)
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task CreateKeys_AllNewKeys_AddsThemAndReturnsKeyIds()
        {
            var request = new CreateKeysRequest
            {
                Keys = new List<CreateKeyItem>
                {
                    new() { KeyName = "key_one", OriginalText = "One", ProjectId = ValidProjectId },
                    new() { KeyName = "key_two", OriginalText = "Two", ProjectId = ValidProjectId }
                }
            };

            _repoMock.Setup(r => r.GetExistingKeys(It.IsAny<List<CreateKeyItem>>()))
                     .ReturnsAsync(new List<(string, int)>());   // none exist yet

            _repoMock.Setup(r => r.AddKeys(It.IsAny<List<TranslationKey>>()))
                     .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CreateKeys(request, EmpId);

            // Assert
            _repoMock.Verify(r => r.AddKeys(It.Is<List<TranslationKey>>(list =>
                list.Count == 2
            )), Times.Once);
        }

        [TestMethod]
        public async Task CreateKeys_AllKeysAlreadyExist_ReturnsNoNewKeysMessage()
        {
            var request = new CreateKeysRequest
            {
                Keys = new List<CreateKeyItem>
                {
                    new() { KeyName = "existing_key", OriginalText = "Existing", ProjectId = ValidProjectId }
                }
            };

            _repoMock.Setup(r => r.GetExistingKeys(It.IsAny<List<CreateKeyItem>>()))
                     .ReturnsAsync(new List<(string, int)> { ("EXISTING_KEY", ValidProjectId) });

            // Act
            dynamic result = await _sut.CreateKeys(request, EmpId);

            // Assert – AddKeys should not be called when everything is a duplicate
            _repoMock.Verify(r => r.AddKeys(It.IsAny<List<TranslationKey>>()), Times.Never);
        }

        [TestMethod]
        public async Task CreateKeys_EmptyList_ThrowsException()
        {
            var request = new CreateKeysRequest { Keys = new List<CreateKeyItem>() };

            await AssertThrowsExceptionAsync(
                () => _sut.CreateKeys(request, EmpId));
        }

        [TestMethod]
        public async Task CreateKeys_MissingKeyName_ThrowsException()
        {
            var request = new CreateKeysRequest
            {
                Keys = new List<CreateKeyItem>
                {
                    new() { KeyName = "", OriginalText = "Some text", ProjectId = ValidProjectId }
                }
            };

            await AssertThrowsExceptionAsync(
                () => _sut.CreateKeys(request, EmpId));
        }

        [TestMethod]
        public async Task CreateKeys_MissingOriginalText_ThrowsException()
        {
            var request = new CreateKeysRequest
            {
                Keys = new List<CreateKeyItem>
                {
                    new() { KeyName = "valid_key", OriginalText = "", ProjectId = ValidProjectId }
                }
            };

            await AssertThrowsExceptionAsync(
                () => _sut.CreateKeys(request, EmpId));
        }

        // ═════════════════════════════════════════════════════════════════════
        // InsertTranslationAsync  (single)
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task InsertTranslation_ValidRequest_SavesAndReturnsSuccess()
        {
            var request = new AddTranslationRequest
            {
                KeyName = ValidKeyName,
                LanguageCode = "en",
                Value = "Hello"
            };

            _repoMock.Setup(r => r.GetKeyIdByNameAsync(ValidKeyName)).ReturnsAsync(ValidKeyId);
            _repoMock.Setup(r => r.LanguageExistsAsync("EN")).ReturnsAsync(true);
            _repoMock.Setup(r => r.GetTranslationValueAsync(ValidKeyId, "EN")).ReturnsAsync((TranslationValue?)null);
            _repoMock.Setup(r => r.SaveTranslationAsync(It.IsAny<TranslationValue>())).Returns(Task.CompletedTask);

            var result = await _sut.InsertTranslationAsync(request, EmpId);

            Assert.AreEqual("Translation saved successfully.", result);
            _repoMock.Verify(r => r.SaveTranslationAsync(It.Is<TranslationValue>(tv =>
                tv.TranslationKeyId == ValidKeyId &&
                tv.LanguageCode == "EN" &&
                tv.Value == "Hello" &&
                tv.UpdatedByEmpId == EmpId
            )), Times.Once);
        }

        [TestMethod]
        public async Task InsertTranslation_InvalidKeyId_ThrowsException()
        {
            var request = new AddTranslationRequest { KeyName = "", LanguageCode = "EN", Value = "Hello" };

            await AssertThrowsExceptionAsync(
                () => _sut.InsertTranslationAsync(request, EmpId));
        }

        [TestMethod]
        public async Task InsertTranslation_MissingLanguageCode_ThrowsException()
        {
            var request = new AddTranslationRequest { KeyName = ValidKeyName, LanguageCode = "", Value = "Hello" };

            await AssertThrowsExceptionAsync(
                () => _sut.InsertTranslationAsync(request, EmpId));
        }

        [TestMethod]
        public async Task InsertTranslation_KeyNotFound_ThrowsException()
        {
            var request = new AddTranslationRequest { KeyName = ValidKeyName, LanguageCode = "EN", Value = "Hello" };

            _repoMock.Setup(r => r.GetKeyIdByNameAsync(ValidKeyName)).ReturnsAsync((int?)null);

            await AssertThrowsExceptionAsync(
                () => _sut.InsertTranslationAsync(request, EmpId));
        }

        [TestMethod]
        public async Task InsertTranslation_UnsupportedLanguage_ThrowsException()
        {
            var request = new AddTranslationRequest { KeyName = ValidKeyName, LanguageCode = "XX", Value = "Hello" };

            _repoMock.Setup(r => r.GetKeyIdByNameAsync(ValidKeyName)).ReturnsAsync(ValidKeyId);
            _repoMock.Setup(r => r.LanguageExistsAsync("XX")).ReturnsAsync(false);

            await AssertThrowsExceptionAsync(
                () => _sut.InsertTranslationAsync(request, EmpId));
        }

        [TestMethod]
        public async Task InsertTranslation_DuplicateTranslation_ThrowsException()
        {
            var request = new AddTranslationRequest { KeyName = ValidKeyName, LanguageCode = "EN", Value = "Hello" };

            _repoMock.Setup(r => r.GetKeyIdByNameAsync(ValidKeyName)).ReturnsAsync(ValidKeyId);
            _repoMock.Setup(r => r.LanguageExistsAsync("EN")).ReturnsAsync(true);
            _repoMock.Setup(r => r.GetTranslationValueAsync(ValidKeyId, "EN"))
                     .ReturnsAsync(new TranslationValue { TranslationKeyId = ValidKeyId, LanguageCode = "EN" });

            await AssertThrowsExceptionAsync(
                () => _sut.InsertTranslationAsync(request, EmpId));

            _repoMock.Verify(r => r.SaveTranslationAsync(It.IsAny<TranslationValue>()), Times.Never);
        }

        // ═════════════════════════════════════════════════════════════════════
        // InsertTranslationsAsync  (bulk)
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task InsertTranslations_AllValidKeys_ReturnsSuccessMessage()
        {
            var request = new BulkTranslationRequest
            {
                Translations = new List<AddTranslationRequest>
                {
                    new() { KeyName = "KEY_ONE", LanguageCode = "en", Value = "Hello" },
                    new() { KeyName = "KEY_TWO", LanguageCode = "fr", Value = "Bonjour" }
                }
            };

            _repoMock.Setup(r => r.GetValidKeyIdsAsync(It.IsAny<List<int>>()))
                     .ReturnsAsync(new List<int> { 1, 2 });

            _repoMock.Setup(r => r.GetExistingTranslationsAsync(It.IsAny<List<int>>()))
                     .ReturnsAsync(new List<TranslationValue>());

            _repoMock.Setup(r => r.InsertBulkAsync(
                It.IsAny<List<BulkTranslationItem>>(),
                It.IsAny<List<TranslationValue>>(),
                EmpId)).Returns(Task.CompletedTask);

            var result = await _sut.InsertTranslationsAsync(request, EmpId);

            Assert.AreEqual("Translations saved successfully.", result);
        }

        [TestMethod]
        public async Task InsertTranslations_SomeInvalidKeys_ReturnsMixedMessage()
        {
            var request = new BulkTranslationRequest
            {
                Translations = new List<AddTranslationRequest>
                {
                    new() { KeyName = "VALID_KEY",   LanguageCode = "en", Value = "Hello" },
                    new() { KeyName = "MISSING_KEY", LanguageCode = "en", Value = "Ghost" }  // invalid
                }
            };

            _repoMock.Setup(r => r.GetValidKeyIdsAsync(It.IsAny<List<int>>()))
                     .ReturnsAsync(new List<int> { 1 });   // only key 1 is valid

            _repoMock.Setup(r => r.GetExistingTranslationsAsync(It.IsAny<List<int>>()))
                     .ReturnsAsync(new List<TranslationValue>());

            _repoMock.Setup(r => r.InsertBulkAsync(
                It.IsAny<List<BulkTranslationItem>>(),
                It.IsAny<List<TranslationValue>>(),
                EmpId)).Returns(Task.CompletedTask);

            var result = await _sut.InsertTranslationsAsync(request, EmpId);

            StringAssert.Contains(result, "Invalid KeyNames: MISSING_KEY");
        }

        [TestMethod]
        public async Task InsertTranslations_AllInvalidKeys_ReturnsNoValidTranslationsMessage()
        {
            var request = new BulkTranslationRequest
            {
                Translations = new List<AddTranslationRequest>
                {
                    new() { KeyName = "INVALID_KEY", LanguageCode = "en", Value = "Test" }
                }
            };

            _repoMock.Setup(r => r.GetValidKeyIdsAsync(It.IsAny<List<int>>()))
                     .ReturnsAsync(new List<int>());   // nothing valid

            var result = await _sut.InsertTranslationsAsync(request, EmpId);

            StringAssert.Contains(result, "No valid translations to save");
            _repoMock.Verify(r => r.InsertBulkAsync(
                It.IsAny<List<BulkTranslationItem>>(),
                It.IsAny<List<TranslationValue>>(),
                It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public async Task InsertTranslations_EmptyList_ThrowsException()
        {
            var request = new BulkTranslationRequest { Translations = new List<AddTranslationRequest>() };

            await AssertThrowsExceptionAsync(
                () => _sut.InsertTranslationsAsync(request, EmpId));
        }

        [TestMethod]
        public async Task InsertTranslations_InvalidKeyId_ThrowsException()
        {
            var request = new BulkTranslationRequest
            {
                Translations = new List<AddTranslationRequest>
                {
                    new() { KeyName = "", LanguageCode = "en", Value = "Hello" }
                }
            };

            await AssertThrowsExceptionAsync(
                () => _sut.InsertTranslationsAsync(request, EmpId));
        }

        [TestMethod]
        public async Task InsertTranslations_MissingLanguageCode_ThrowsException()
        {
            var request = new BulkTranslationRequest
            {
                Translations = new List<AddTranslationRequest>
                {
                    new() { KeyName = "KEY_ONE", LanguageCode = "", Value = "Hello" }
                }
            };

            await AssertThrowsExceptionAsync(() => _sut.InsertTranslationsAsync(request, EmpId));
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetTranslationAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetTranslation_ValidKeyAndLanguage_ReturnsTranslationData()
        {
            var translation = new TranslationValue
            {
                TranslationKeyId = ValidKeyId,
                LanguageCode = LangCode,
                Value = "Hello"
            };

            _repoMock.Setup(r => r.GetKeyIdByNameAsync(ValidKeyName)).ReturnsAsync(ValidKeyId);
            _repoMock.Setup(r => r.GetTranslationForUiAsync(ValidKeyId, LangCode))
                     .ReturnsAsync(translation);

            dynamic result = await _sut.GetTranslationAsync(ValidKeyName, "en");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetTranslation_NotFound_ReturnsEmptyValue()
        {
            _repoMock.Setup(r => r.GetKeyIdByNameAsync(ValidKeyName)).ReturnsAsync(ValidKeyId);
            _repoMock.Setup(r => r.GetTranslationForUiAsync(ValidKeyId, LangCode))
                     .ReturnsAsync((TranslationValue?)null);

            dynamic result = await _sut.GetTranslationAsync(ValidKeyName, "en");

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetTranslation_InvalidKeyId_ThrowsException()
        {
            await AssertThrowsExceptionAsync(() => _sut.GetTranslationAsync("", "EN"));
        }

        [TestMethod]
        public async Task GetTranslation_MissingLanguageCode_ThrowsException()
        {
            await AssertThrowsExceptionAsync(() => _sut.GetTranslationAsync(ValidKeyName, ""));
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetAllTranslationsAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetAllTranslations_ValidKey_ReturnsProjectedList()
        {
            var translations = new List<TranslationValue>
            {
                new() { LanguageCode = "EN", Value = "Hello"   },
                new() { LanguageCode = "FR", Value = "Bonjour" }
            };

            _repoMock.Setup(r => r.GetKeyIdByNameAsync(ValidKeyName)).ReturnsAsync(ValidKeyId);
            _repoMock.Setup(r => r.GetTranslationsByKeyAsync(ValidKeyId))
                     .ReturnsAsync(translations);

            var result = await _sut.GetAllTranslationsAsync(ValidKeyName);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetAllTranslations_InvalidKeyId_ThrowsException()
        {
            _repoMock.Setup(r => r.GetKeyIdByNameAsync("")).ReturnsAsync((int?)null);
            await AssertThrowsExceptionAsync(() => _sut.GetAllTranslationsAsync(""));
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetKeysWithTranslationsAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetKeysWithTranslations_ValidLanguage_ReturnsResults()
        {
            var dtoList = new List<TranslationKeyWithValueDto>
            {
                new() { Key = "WELCOME", Value = "Hello" }
            };

            _repoMock.Setup(r => r.GetKeysWithTranslationsAsync("EN"))
                     .ReturnsAsync(dtoList);

            var result = await _sut.GetKeysWithTranslationsAsync("en");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("WELCOME", result[0].Key);
        }

        [TestMethod]
        public async Task GetKeysWithTranslations_MissingLanguageCode_ThrowsException()
        {
            await AssertThrowsExceptionAsync(() => _sut.GetKeysWithTranslationsAsync(""));
        }

        // ═════════════════════════════════════════════════════════════════════
        // UpsertTranslationsV2Async
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task UpsertTranslationsV2_AllValidKeys_ReturnsSuccessMessage()
        {
            var request = new BulkTranslationRequestV2
            {
                Translations = new List<BulkTranslationItem>
                {
                    new() { KeyId = 1, LanguageCode = "EN", Value = "Hello" }
                }
            };

            _repoMock.Setup(r => r.GetValidKeyIdsAsync(It.IsAny<List<int>>()))
                     .ReturnsAsync(new List<int> { 1 });

            _repoMock.Setup(r => r.GetExistingTranslationsAsync(It.IsAny<List<int>>()))
                     .ReturnsAsync(new List<TranslationValue>());

            _repoMock.Setup(r => r.InsertBulkAsync(
                It.IsAny<List<BulkTranslationItem>>(),
                It.IsAny<List<TranslationValue>>(),
                EmpId)).Returns(Task.CompletedTask);

            var result = await _sut.UpsertTranslationsV2Async(request, EmpId);

            Assert.AreEqual("Translations saved successfully.", result);
        }

        [TestMethod]
        public async Task UpsertTranslationsV2_SomeKeysNotFound_ThrowsException()
        {
            var request = new BulkTranslationRequestV2
            {
                Translations = new List<BulkTranslationItem>
                {
                    new() { KeyId = 1,   LanguageCode = "EN", Value = "Hello" },
                    new() { KeyId = 999, LanguageCode = "EN", Value = "Ghost" }
                }
            };

            // Only key 1 comes back as valid — count mismatch triggers exception
            _repoMock.Setup(r => r.GetValidKeyIdsAsync(It.IsAny<List<int>>()))
                     .ReturnsAsync(new List<int> { 1 });

            await AssertThrowsExceptionAsync(() => _sut.UpsertTranslationsV2Async(request, EmpId));
        }

        [TestMethod]
        public async Task UpsertTranslationsV2_EmptyList_ThrowsException()
        {
            var request = new BulkTranslationRequestV2 { Translations = new List<BulkTranslationItem>() };

            await AssertThrowsExceptionAsync(() => _sut.UpsertTranslationsV2Async(request, EmpId));
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetAllKeys
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetAllKeys_ReturnsProjectedKeyList()
        {
            var keys = new List<TranslationKey>
            {
                new() { KeyName = "KEY_ONE", OriginalText = "One", ProjectId = 1 },
                new() { KeyName = "KEY_TWO", OriginalText = "Two", ProjectId = 2 }
            };

            _repoMock.Setup(r => r.GetAllKeys()).ReturnsAsync(keys);

            var result = await _sut.GetAllKeys();

            Assert.IsNotNull(result);
            _repoMock.Verify(r => r.GetAllKeys(), Times.Once);
        }

        // ═════════════════════════════════════════════════════════════════════
        // DeleteKey
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task DeleteKey_ValidKeyWithTranslations_DeletesValuesAndKey()
        {
            var translations = new List<TranslationValue>
            {
                new() { TranslationKeyId = ValidKeyId, LanguageCode = "EN", Value = "Hello" }
            };

            var key = new TranslationKey
            {
                Id = ValidKeyId,
                KeyName = "SOME_KEY",
                OriginalText = "Some text",
                ProjectId = ValidProjectId,
                Translations = translations
            };

            _repoMock.Setup(r => r.GetKeyByIdAsync(ValidKeyId)).ReturnsAsync(key);
            _repoMock.Setup(r => r.DeleteValuesAsync(It.IsAny<List<TranslationValue>>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.DeleteKeyAsync(key)).Returns(Task.CompletedTask);

            await _sut.DeleteKey(ValidKeyId);

            _repoMock.Verify(r => r.DeleteValuesAsync(It.IsAny<List<TranslationValue>>()), Times.Once);
            _repoMock.Verify(r => r.DeleteKeyAsync(key), Times.Once);
        }

        [TestMethod]
        public async Task DeleteKey_ValidKeyWithNoTranslations_DeletesKeyOnly()
        {
            var key = new TranslationKey
            {
                Id = ValidKeyId,
                KeyName = "SOME_KEY",
                OriginalText = "Some text",
                ProjectId = ValidProjectId,
                Translations = new List<TranslationValue>()  // empty
            };

            _repoMock.Setup(r => r.GetKeyByIdAsync(ValidKeyId)).ReturnsAsync(key);
            _repoMock.Setup(r => r.DeleteKeyAsync(key)).Returns(Task.CompletedTask);

            await _sut.DeleteKey(ValidKeyId);

            _repoMock.Verify(r => r.DeleteValuesAsync(It.IsAny<List<TranslationValue>>()), Times.Never);
            _repoMock.Verify(r => r.DeleteKeyAsync(key), Times.Once);
        }

        [TestMethod]
        public async Task DeleteKey_InvalidId_ThrowsException()
        {
            await AssertThrowsExceptionAsync(() => _sut.DeleteKey(0));
        }

        [TestMethod]
        public async Task DeleteKey_KeyNotFound_ThrowsException()
        {
            _repoMock.Setup(r => r.GetKeyByIdAsync(ValidKeyId)).ReturnsAsync((TranslationKey?)null);

            await AssertThrowsExceptionAsync(() => _sut.DeleteKey(ValidKeyId));
        }

        // ═════════════════════════════════════════════════════════════════════
        // PublishTranslationsAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task PublishTranslations_NoTranslationsExist_ReturnsFailureResponse()
        {
            _repoMock.Setup(r => r.GetAllTranslationsForPublishAsync())
                     .ReturnsAsync(new List<TranslationValue>());

            var result = await _sut.PublishTranslationsAsync();

            Assert.IsFalse(result.Success);
            Assert.AreEqual("No translations found", result.Message);
        }

        [TestMethod]
        public async Task PublishTranslations_WithTranslations_CreatesFilesAndSavesRecord()
        {
            // Arrange – build minimal TranslationValue objects with navigation props
            var key = new TranslationKey { KeyName = "WELCOME", OriginalText = "Welcome", ProjectId = 1 };
            var lang = new Language { Code = "EN", Name = "English" };

            var translations = new List<TranslationValue>
            {
                new()
                {
                    TranslationKeyId = 1,
                    LanguageCode     = "EN",
                    Value            = "Hello",
                    TranslationKey   = key,
                    Language         = lang
                }
            };

            _repoMock.Setup(r => r.GetAllTranslationsForPublishAsync())
                     .ReturnsAsync(translations);

            _repoMock.Setup(r => r.SavePublishRecordAsync(It.IsAny<TranslationPublish>()))
                     .Returns(Task.CompletedTask);

            // Point the env to a real temp folder so file writes succeed
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            _envMock.Setup(e => e.ContentRootPath).Returns(tempDir);

            try
            {
                // Act
                var result = await _sut.PublishTranslationsAsync();

                // Assert
                Assert.IsTrue(result.Success);
                Assert.AreEqual("Translations published successfully", result.Message);
                Assert.IsTrue(result.FileCount > 0);

                _repoMock.Verify(r => r.SavePublishRecordAsync(It.Is<TranslationPublish>(p =>
                    p.FileCount > 0 &&
                    !string.IsNullOrEmpty(p.Version)
                )), Times.Once);
            }
            finally
            {
                // Cleanup temp folder
                Directory.Delete(tempDir, recursive: true);
            }
        }

        [TestMethod]
        public async Task PublishTranslations_VersionFollowsDateFormat()
        {
            var key = new TranslationKey { KeyName = "KEY", OriginalText = "text", ProjectId = 1 };
            var lang = new Language { Code = "DE", Name = "German" };

            var translations = new List<TranslationValue>
            {
                new() { TranslationKeyId = 1, LanguageCode = "DE", Value = "Hallo",
                        TranslationKey = key, Language = lang }
            };

            _repoMock.Setup(r => r.GetAllTranslationsForPublishAsync()).ReturnsAsync(translations);
            _repoMock.Setup(r => r.SavePublishRecordAsync(It.IsAny<TranslationPublish>())).Returns(Task.CompletedTask);

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            _envMock.Setup(e => e.ContentRootPath).Returns(tempDir);

            try
            {
                var result = await _sut.PublishTranslationsAsync();

                // Version should start with "v" followed by digits
                Assert.IsTrue(result.Version.StartsWith("v"));
                Assert.IsTrue(result.Version.Length > 1);
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}