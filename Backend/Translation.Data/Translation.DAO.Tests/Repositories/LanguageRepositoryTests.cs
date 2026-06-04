using Translation.DAO.Data;
using Translation.DAO.Repositories;
using Translation.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Translation.DAO.Tests.Repositories
{
    [TestClass]
    public class LanguageRepositoryTests
    {
        private AppDbContext _context = null!;
        private LanguageRepository _sut = null!;

        [TestInitialize]
        public void SetUp()
        {
            // Each test gets a fresh in-memory database
            _context = TestDbContextFactory.Create();
            _sut = new LanguageRepository(_context);
        }

        [TestCleanup]
        public void TearDown()
        {
            _context.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetLanguagesAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetLanguages_EmptyDatabase_ReturnsEmptyList()
        {
            var result = await _sut.GetLanguagesAsync();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetLanguages_WithSeededData_ReturnsAllLanguages()
        {
            _context.Languages.AddRange(
                new Language { Code = "EN", Name = "English" },
                new Language { Code = "FR", Name = "French" },
                new Language { Code = "DE", Name = "German" }
            );
            await _context.SaveChangesAsync();

            var result = await _sut.GetLanguagesAsync();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public async Task GetLanguages_ReturnsCorrectLanguageCodes()
        {
            _context.Languages.AddRange(
                new Language { Code = "EN", Name = "English" },
                new Language { Code = "JA", Name = "Japanese" }
            );
            await _context.SaveChangesAsync();

            var result = await _sut.GetLanguagesAsync();

            CollectionAssert.Contains(result.Select(l => l.Code).ToList(), "EN");
            CollectionAssert.Contains(result.Select(l => l.Code).ToList(), "JA");
        }

        // ═════════════════════════════════════════════════════════════════════
        // AddLanguageAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task AddLanguage_ValidLanguage_PersistsToDatabase()
        {
            var language = new Language { Code = "ES", Name = "Spanish" };

            await _sut.AddLanguageAsync(language);

            var saved = await _context.Languages.FindAsync(language.Id);
            Assert.IsNotNull(saved);
            Assert.AreEqual("ES", saved.Code);
            Assert.AreEqual("Spanish", saved.Name);
        }

        [TestMethod]
        public async Task AddLanguage_AssignsIdAfterSave()
        {
            var language = new Language { Code = "KO", Name = "Korean" };

            await _sut.AddLanguageAsync(language);

            Assert.IsTrue(language.Id > 0);
        }

        [TestMethod]
        public async Task AddLanguage_MultipleLanguages_AllPersist()
        {
            await _sut.AddLanguageAsync(new Language { Code = "EN", Name = "English" });
            await _sut.AddLanguageAsync(new Language { Code = "FR", Name = "French" });

            var all = await _context.Languages.ToListAsync();
            Assert.AreEqual(2, all.Count);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetLanguageByIdAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetLanguageById_ExistingId_ReturnsCorrectLanguage()
        {
            var language = new Language { Code = "EN", Name = "English" };
            _context.Languages.Add(language);
            await _context.SaveChangesAsync();

            var result = await _sut.GetLanguageByIdAsync(language.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("EN", result.Code);
            Assert.AreEqual("English", result.Name);
        }

        [TestMethod]
        public async Task GetLanguageById_NonExistingId_ReturnsNull()
        {
            var result = await _sut.GetLanguageByIdAsync(999);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetLanguageById_ReturnsCorrectLanguageWhenMultipleExist()
        {
            var lang1 = new Language { Code = "EN", Name = "English" };
            var lang2 = new Language { Code = "FR", Name = "French" };
            _context.Languages.AddRange(lang1, lang2);
            await _context.SaveChangesAsync();

            var result = await _sut.GetLanguageByIdAsync(lang2.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("FR", result.Code);
        }

        // ═════════════════════════════════════════════════════════════════════
        // DeleteLanguageAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task DeleteLanguage_ExistingLanguage_RemovesFromDatabase()
        {
            var language = new Language { Code = "EN", Name = "English" };
            _context.Languages.Add(language);
            await _context.SaveChangesAsync();

            await _sut.DeleteLanguageAsync(language);

            var deleted = await _context.Languages.FindAsync(language.Id);
            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task DeleteLanguage_OnlyDeletesTargetLanguage()
        {
            var lang1 = new Language { Code = "EN", Name = "English" };
            var lang2 = new Language { Code = "FR", Name = "French" };
            _context.Languages.AddRange(lang1, lang2);
            await _context.SaveChangesAsync();

            await _sut.DeleteLanguageAsync(lang1);

            var remaining = await _context.Languages.ToListAsync();
            Assert.AreEqual(1, remaining.Count);
            Assert.AreEqual("FR", remaining[0].Code);
        }
    }
}