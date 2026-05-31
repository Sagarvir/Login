using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Translation.Contracts.DTO.Languages;
using Translation.DAO.Repositories.Interfaces;
using Translation.Models.Entities;
using Translation.Service.Services;

namespace Translation.Tests.Services
{
    [TestClass]
    public class LanguageServiceTests
    {
        private Mock<ILanguageRepository> _repoMock = null!;
        private LanguageService _sut = null!;

        [TestInitialize]
        public void SetUp()
        {
            _repoMock = new Mock<ILanguageRepository>();
            _sut = new LanguageService(_repoMock.Object);
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
        // GetLanguagesAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetLanguages_ReturnsAllLanguagesFromRepo()
        {
            var languages = new List<Language>
            {
                new() { Id = 1, Code = "EN", Name = "English" },
                new() { Id = 2, Code = "FR", Name = "French"  },
                new() { Id = 3, Code = "DE", Name = "German"  }
            };

            _repoMock.Setup(r => r.GetLanguagesAsync()).ReturnsAsync(languages);

            var result = await _sut.GetLanguagesAsync();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("EN", result[0].Code);
            Assert.AreEqual("FR", result[1].Code);
            _repoMock.Verify(r => r.GetLanguagesAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetLanguages_EmptyRepo_ReturnsEmptyList()
        {
            _repoMock.Setup(r => r.GetLanguagesAsync()).ReturnsAsync(new List<Language>());

            var result = await _sut.GetLanguagesAsync();

            Assert.AreEqual(0, result.Count);
        }

        // ═════════════════════════════════════════════════════════════════════
        // AddLanguageAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task AddLanguage_ValidRequest_AddsAndReturnsLanguage()
        {
            var dto = new AddLanguage { code = "JA", name = "Japanese" };

            _repoMock.Setup(r => r.AddLanguageAsync(It.IsAny<Language>()))
                     .Returns(Task.CompletedTask);

            var result = await _sut.AddLanguageAsync(dto);

            Assert.IsNotNull(result);
            Assert.AreEqual("JA", result.Code);
            Assert.AreEqual("Japanese", result.Name);

            _repoMock.Verify(r => r.AddLanguageAsync(It.Is<Language>(l =>
                l.Code == "JA" && l.Name == "Japanese"
            )), Times.Once);
        }

        [TestMethod]
        public async Task AddLanguage_MissingCode_ThrowsException()
        {
            var dto = new AddLanguage { code = "", name = "Japanese" };

            await AssertThrowsExceptionAsync(() => _sut.AddLanguageAsync(dto));

            _repoMock.Verify(r => r.AddLanguageAsync(It.IsAny<Language>()), Times.Never);
        }

        [TestMethod]
        public async Task AddLanguage_WhitespaceCode_ThrowsException()
        {
            var dto = new AddLanguage { code = "   ", name = "Japanese" };

            await AssertThrowsExceptionAsync(() => _sut.AddLanguageAsync(dto));
        }

        [TestMethod]
        public async Task AddLanguage_MissingName_ThrowsException()
        {
            var dto = new AddLanguage { code = "JA", name = "" };

            await AssertThrowsExceptionAsync(() => _sut.AddLanguageAsync(dto));

            _repoMock.Verify(r => r.AddLanguageAsync(It.IsAny<Language>()), Times.Never);
        }

        [TestMethod]
        public async Task AddLanguage_WhitespaceName_ThrowsException()
        {
            var dto = new AddLanguage { code = "JA", name = "   " };

            await AssertThrowsExceptionAsync(() => _sut.AddLanguageAsync(dto));
        }

        [TestMethod]
        public async Task AddLanguage_LanguageEntityHasCorrectProperties()
        {
            var dto = new AddLanguage { code = "ES", name = "Spanish" };
            Language? capturedLang = null;

            _repoMock.Setup(r => r.AddLanguageAsync(It.IsAny<Language>()))
                     .Callback<Language>(l => capturedLang = l)
                     .Returns(Task.CompletedTask);

            await _sut.AddLanguageAsync(dto);

            Assert.IsNotNull(capturedLang);
            Assert.AreEqual("ES", capturedLang!.Code);
            Assert.AreEqual("Spanish", capturedLang.Name);
        }

        // ═════════════════════════════════════════════════════════════════════
        // DeleteLanguageAsync
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task DeleteLanguage_ValidId_DeletesSuccessfully()
        {
            var lang = new Language { Id = 1, Code = "EN", Name = "English" };

            _repoMock.Setup(r => r.GetLanguageByIdAsync(1)).ReturnsAsync(lang);
            _repoMock.Setup(r => r.DeleteLanguageAsync(lang)).Returns(Task.CompletedTask);

            await _sut.DeleteLanguageAsync(1);

            _repoMock.Verify(r => r.GetLanguageByIdAsync(1), Times.Once);
            _repoMock.Verify(r => r.DeleteLanguageAsync(lang), Times.Once);
        }

        [TestMethod]
        public async Task DeleteLanguage_LanguageNotFound_ThrowsException()
        {
            _repoMock.Setup(r => r.GetLanguageByIdAsync(99)).ReturnsAsync((Language?)null);

            await AssertThrowsExceptionAsync(() => _sut.DeleteLanguageAsync(99));

            _repoMock.Verify(r => r.DeleteLanguageAsync(It.IsAny<Language>()), Times.Never);
        }

        [TestMethod]
        public async Task DeleteLanguage_PassesCorrectEntityToRepo()
        {
            var lang = new Language { Id = 5, Code = "FR", Name = "French" };

            _repoMock.Setup(r => r.GetLanguageByIdAsync(5)).ReturnsAsync(lang);
            _repoMock.Setup(r => r.DeleteLanguageAsync(It.IsAny<Language>())).Returns(Task.CompletedTask);

            await _sut.DeleteLanguageAsync(5);

            // Verify the exact object retrieved from the repo is passed to Delete
            _repoMock.Verify(r => r.DeleteLanguageAsync(It.Is<Language>(l =>
                l.Id == 5 && l.Code == "FR"
            )), Times.Once);
        }
    }
}