using Microsoft.AspNetCore.Mvc;
using Moq;
using Translation.API.Controllers;
using Translation.Contracts.DTO.Translation;
using Translation.Models.Entities;
using Translation.Service.Interfaces;

namespace Translation.API.Tests.Controllers
{
    [TestClass]
    public class TranslationValueControllerTests
    {
        private Mock<ITranslationService> _serviceMock = null!;
        private TranslationValueController _sut = null!;

        private const string EmpId = "emp001";

        [TestInitialize]
        public void SetUp()
        {
            _serviceMock = new Mock<ITranslationService>();
            _sut = new TranslationValueController(_serviceMock.Object);
            ControllerTestHelper.SetUser(_sut, empId: EmpId, role: "Translator");
        }

        // ═════════════════════════════════════════════════════════════════════
        // InsertTranslation (single)
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task InsertTranslation_ValidRequest_Returns200WithMessage()
        {
            var request = new AddTranslationRequest { KeyName = "hello", LanguageCode = "EN", Value = "Hello" };

            _serviceMock.Setup(s => s.InsertTranslationAsync(request, EmpId))
                        .ReturnsAsync("Translation saved successfully.");

            var result = await _sut.InsertTranslation(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Translation saved successfully.", ok.Value);
        }

        [TestMethod]
        public async Task InsertTranslation_ServiceThrows_Returns400WithMessage()
        {
            var request = new AddTranslationRequest { KeyName ="hello" , LanguageCode = "EN", Value = "Hello" };

            _serviceMock.Setup(s => s.InsertTranslationAsync(request, EmpId))
                        .ThrowsAsync(new Exception("Valid KeyId is required."));

            var result = await _sut.InsertTranslation(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("Valid KeyId is required.", bad.Value);
        }

        [TestMethod]
        public async Task InsertTranslation_DuplicateTranslation_Returns400()
        {
            var request = new AddTranslationRequest { KeyName = "hello", LanguageCode = "EN", Value = "Hello" };

            _serviceMock.Setup(s => s.InsertTranslationAsync(request, EmpId))
                        .ThrowsAsync(new Exception("Translation already exists for KeyId 1 and Language 'EN'."));

            var result = await _sut.InsertTranslation(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task InsertTranslation_PassesEmpIdFromClaimsToService()
        {
            var request = new AddTranslationRequest { KeyName = "hello", LanguageCode = "EN", Value = "Hello" };

            _serviceMock.Setup(s => s.InsertTranslationAsync(request, EmpId))
                        .ReturnsAsync("Translation saved successfully.");

            await _sut.InsertTranslation(request);

            _serviceMock.Verify(s => s.InsertTranslationAsync(request, EmpId), Times.Once);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetTranslation
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetTranslation_ValidKeyAndLanguage_Returns200WithData()
        {
            var response = new { TranslationKeyId = 1, LanguageCode = "EN", Value = "Hello" };

            _serviceMock.Setup(s => s.GetTranslationAsync("WELCOME", "EN"))
                        .ReturnsAsync(response);

            var result = await _sut.GetTranslation("WELCOME", "EN");

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(response, ok.Value);
        }

        [TestMethod]
        public async Task GetTranslation_NotFound_Returns200WithEmptyValue()
        {
            var response = new { value = "" };

            _serviceMock.Setup(s => s.GetTranslationAsync("MISSING_KEY", "EN"))
                        .ReturnsAsync(response);

            var result = await _sut.GetTranslation("MISSING_KEY", "EN");

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetTranslation_ServiceThrows_Returns400()
        {
            _serviceMock.Setup(s => s.GetTranslationAsync("", "EN"))
                        .ThrowsAsync(new Exception("Valid KeyId is required."));

            var result = await _sut.GetTranslation("", "EN");

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetAllTranslations
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetAllTranslations_ValidKeyName_Returns200WithList()
        {
            var response = new List<object>
            {
                new { LanguageCode = "EN", Value = "Hello"   },
                new { LanguageCode = "FR", Value = "Bonjour" }
            };

            _serviceMock.Setup(s => s.GetAllTranslationsAsync("WELCOME"))
                        .ReturnsAsync(response);

            var result = await _sut.GetAllTranslations("WELCOME");

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(response, ok.Value);
        }

        [TestMethod]
        public async Task GetAllTranslations_ServiceThrows_Returns400()
        {
            _serviceMock.Setup(s => s.GetAllTranslationsAsync(""))
                        .ThrowsAsync(new Exception("LanguageCode is required."));

            var result = await _sut.GetAllTranslations("");

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        // ═════════════════════════════════════════════════════════════════════
        // InsertTranslations (bulk)
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task InsertTranslations_ValidRequest_Returns200WithMessage()
        {
            var request = new BulkTranslationRequest
            {
                Translations = new List<AddTranslationRequest>
                {
                    new() { KeyName = "hello", LanguageCode = "EN", Value = "Hello" }
                }
            };

            _serviceMock.Setup(s => s.InsertTranslationsAsync(request, EmpId))
                        .ReturnsAsync("Translations saved successfully.");

            var result = await _sut.InsertTranslations(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Translations saved successfully.", ok.Value);
        }

        [TestMethod]
        public async Task InsertTranslations_ServiceThrows_Returns400()
        {
            var request = new BulkTranslationRequest { Translations = new List<AddTranslationRequest>() };

            _serviceMock.Setup(s => s.InsertTranslationsAsync(request, EmpId))
                        .ThrowsAsync(new Exception("At least one translation is required."));

            var result = await _sut.InsertTranslations(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task InsertTranslations_PassesEmpIdFromClaimsToService()
        {
            var request = new BulkTranslationRequest
            {
                Translations = new List<AddTranslationRequest>
                {
                    new() { KeyName = "hello", LanguageCode = "EN", Value = "Hello" }
                }
            };

            _serviceMock.Setup(s => s.InsertTranslationsAsync(request, EmpId))
                        .ReturnsAsync("Translations saved successfully.");

            await _sut.InsertTranslations(request);

            _serviceMock.Verify(s => s.InsertTranslationsAsync(request, EmpId), Times.Once);
        }

        // ═════════════════════════════════════════════════════════════════════
        // UpsertTranslationsV2
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task UpsertTranslationsV2_ValidRequest_Returns200()
        {
            var request = new BulkTranslationRequestV2
            {
                Translations = new List<BulkTranslationItem>
                {
                    new() { KeyId = 1, LanguageCode = "EN", Value = "Hello" }
                }
            };

            _serviceMock.Setup(s => s.UpsertTranslationsV2Async(request, EmpId))
                        .ReturnsAsync("Translations saved successfully.");

            var result = await _sut.UpsertTranslationsV2(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task UpsertTranslationsV2_ServiceThrows_Returns400()
        {
            var request = new BulkTranslationRequestV2 { Translations = new List<BulkTranslationItem>() };

            _serviceMock.Setup(s => s.UpsertTranslationsV2Async(request, EmpId))
                        .ThrowsAsync(new Exception("At least one translation is required."));

            var result = await _sut.UpsertTranslationsV2(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetKeysWithTranslations
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetKeysWithTranslations_ValidLanguage_Returns200WithList()
        {
            var dtoList = new List<TranslationKeyWithValueDto>
            {
                new() { Key = "WELCOME", Value = "Hello", OriginalText = "Welcome" }
            };

            _serviceMock.Setup(s => s.GetKeysWithTranslationsAsync("EN"))
                        .ReturnsAsync(dtoList);

            var result = await _sut.GetKeysWithTranslations("EN");

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(dtoList, ok.Value);
        }

        [TestMethod]
        public async Task GetKeysWithTranslations_ServiceThrows_Returns400()
        {
            _serviceMock.Setup(s => s.GetKeysWithTranslationsAsync(""))
                        .ThrowsAsync(new Exception("LanguageCode is required."));

            var result = await _sut.GetKeysWithTranslations("");

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        // ═════════════════════════════════════════════════════════════════════
        // PublishTranslations
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task PublishTranslations_Success_Returns200WithResponse()
        {
            var response = new PublishTranslationResponse
            {
                Success = true,
                Version = "v20250101120000",
                FileCount = 4,
                Message = "Translations published successfully"
            };

            _serviceMock.Setup(s => s.PublishTranslationsAsync()).ReturnsAsync(response);

            var result = await _sut.PublishTranslations();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(response, ok.Value);
        }

        [TestMethod]
        public async Task PublishTranslations_NoTranslationsFound_Returns400WithResponse()
        {
            var response = new PublishTranslationResponse
            {
                Success = false,
                Message = "No translations found"
            };

            _serviceMock.Setup(s => s.PublishTranslationsAsync()).ReturnsAsync(response);

            var result = await _sut.PublishTranslations();

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual(response, bad.Value);
        }

        // ═════════════════════════════════════════════════════════════════════
        // PublishLanguage
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task PublishLanguage_ValidCode_Returns200WithResponse()
        {
            var response = new PublishTranslationResponse
            {
                Success = true,
                Version = "v20250101120000",
                FileCount = 2,
                Message = "Translations published successfully"
            };

            _serviceMock.Setup(s => s.PublishLanguageAsync("EN")).ReturnsAsync(response);

            var result = await _sut.PublishLanguage("EN");

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task PublishLanguage_NoTranslationsForLanguage_Returns400()
        {
            var response = new PublishTranslationResponse
            {
                Success = false,
                Message = "No translations found"
            };

            _serviceMock.Setup(s => s.PublishLanguageAsync("XX")).ReturnsAsync(response);

            var result = await _sut.PublishLanguage("XX");

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task PublishLanguage_CallsServiceWithCorrectLanguageCode()
        {
            _serviceMock.Setup(s => s.PublishLanguageAsync("FR"))
                        .ReturnsAsync(new PublishTranslationResponse { Success = true });

            await _sut.PublishLanguage("FR");

            _serviceMock.Verify(s => s.PublishLanguageAsync("FR"), Times.Once);
        }
    }
}