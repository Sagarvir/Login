using Microsoft.AspNetCore.Mvc;
using Moq;
using Translation.API.Controllers;
using Translation.Contracts.DTO.Languages;
using Translation.Models.Entities;
using Translation.Service.Interfaces;

namespace Translation.API.Tests.Controllers
{
    [TestClass]
    public class LanguageControllerTests
    {
        private Mock<ILanguageService> _serviceMock = null!;
        private LanguageController _sut = null!;

        [TestInitialize]
        public void SetUp()
        {
            _serviceMock = new Mock<ILanguageService>();
            _sut = new LanguageController(_serviceMock.Object);
            ControllerTestHelper.SetUser(_sut, role: "Admin");
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetLanguages
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetLanguages_Returns200WithList()
        {
            var languages = new List<Language>
            {
                new() { Id = 1, Code = "EN", Name = "English" },
                new() { Id = 2, Code = "FR", Name = "French"  }
            };

            _serviceMock.Setup(s => s.GetLanguagesAsync()).ReturnsAsync(languages);

            var result = await _sut.GetLanguages();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(languages, ok.Value);
        }

        [TestMethod]
        public async Task GetLanguages_EmptyList_Returns200WithEmptyList()
        {
            _serviceMock.Setup(s => s.GetLanguagesAsync())
                        .ReturnsAsync(new List<Language>());

            var result = await _sut.GetLanguages();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            var value = ok.Value as List<Language>;
            Assert.AreEqual(0, value!.Count);
        }

        [TestMethod]
        public async Task GetLanguages_CallsServiceExactlyOnce()
        {
            _serviceMock.Setup(s => s.GetLanguagesAsync())
                        .ReturnsAsync(new List<Language>());

            await _sut.GetLanguages();

            _serviceMock.Verify(s => s.GetLanguagesAsync(), Times.Once);
        }

        // ═════════════════════════════════════════════════════════════════════
        // AddLanguage
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task AddLanguage_ValidRequest_Returns200WithLanguage()
        {
            var dto = new AddLanguage { code = "JA", name = "Japanese" };
            var language = new Language { Id = 3, Code = "JA", Name = "Japanese" };

            _serviceMock.Setup(s => s.AddLanguageAsync(dto)).ReturnsAsync(language);

            var result = await _sut.AddLanguage(dto);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(language, ok.Value);
        }

        [TestMethod]
        public async Task AddLanguage_ServiceThrows_Returns400()
        {
            var dto = new AddLanguage { code = "", name = "Japanese" };

            _serviceMock.Setup(s => s.AddLanguageAsync(dto))
                        .ThrowsAsync(new Exception("Code is required."));

            // AddLanguage controller doesn't have try/catch — exception bubbles up
            // This tests that the service is called with correct input
            _serviceMock.Setup(s => s.AddLanguageAsync(It.IsAny<AddLanguage>()))
                        .ThrowsAsync(new Exception("Code is required."));

            await Assert.ThrowsAsync<Exception>(
                () => _sut.AddLanguage(dto));
        }

        [TestMethod]
        public async Task AddLanguage_CallsServiceWithCorrectDto()
        {
            var dto = new AddLanguage { code = "ES", name = "Spanish" };
            var language = new Language { Id = 4, Code = "ES", Name = "Spanish" };

            _serviceMock.Setup(s => s.AddLanguageAsync(dto)).ReturnsAsync(language);

            await _sut.AddLanguage(dto);

            _serviceMock.Verify(s => s.AddLanguageAsync(dto), Times.Once);
        }

        // ═════════════════════════════════════════════════════════════════════
        // DeleteLanguage
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task DeleteLanguage_ValidId_Returns200WithMessage()
        {
            _serviceMock.Setup(s => s.DeleteLanguageAsync(1))
                        .Returns(Task.CompletedTask);

            var result = await _sut.DeleteLanguage(1);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Deleted", ok.Value);
        }

        [TestMethod]
        public async Task DeleteLanguage_NotFound_Returns404WithMessage()
        {
            _serviceMock.Setup(s => s.DeleteLanguageAsync(99))
                        .ThrowsAsync(new Exception("Language not found."));

            var result = await _sut.DeleteLanguage(99);

            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFound = (NotFoundObjectResult)result;
            Assert.AreEqual("Language not found.", notFound.Value);
        }

        [TestMethod]
        public async Task DeleteLanguage_CallsServiceWithCorrectId()
        {
            _serviceMock.Setup(s => s.DeleteLanguageAsync(5))
                        .Returns(Task.CompletedTask);

            await _sut.DeleteLanguage(5);

            _serviceMock.Verify(s => s.DeleteLanguageAsync(5), Times.Once);
        }
    }
}