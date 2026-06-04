using Microsoft.AspNetCore.Mvc;
using Moq;
using Translation.API.Controllers;
using Translation.Contracts.DTO.Translation;
using Translation.Service.Interfaces;

namespace Translation.API.Tests.Controllers
{
    [TestClass]
    public class TranslationKeyControllerTests
    {
        private Mock<ITranslationService> _serviceMock = null!;
        private TranslationKeyController _sut = null!;

        private const string EmpId = "emp001";

        [TestInitialize]
        public void SetUp()
        {
            _serviceMock = new Mock<ITranslationService>();
            _sut = new TranslationKeyController(_serviceMock.Object);
            // Set empId claim so User.FindFirst("empId")?.Value returns "emp001"
            ControllerTestHelper.SetUser(_sut, empId: EmpId, role: "Creator");
        }

        // ═════════════════════════════════════════════════════════════════════
        // CreateKey
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task CreateKey_ValidRequest_Returns200WithResult()
        {
            var request = new CreateKeyRequest { KeyName = "WELCOME", OriginalText = "Welcome", ProjectId = 1 };
            var response = new { message = "Key created successfully.", keyId = 1 };

            _serviceMock.Setup(s => s.CreateKey(request, EmpId))
                        .ReturnsAsync(response);

            var result = await _sut.CreateKey(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(response, ok.Value);
        }

        [TestMethod]
        public async Task CreateKey_ServiceThrows_Returns400WithMessage()
        {
            var request = new CreateKeyRequest { KeyName = "DUPLICATE", OriginalText = "text", ProjectId = 1 };

            _serviceMock.Setup(s => s.CreateKey(request, EmpId))
                        .ThrowsAsync(new Exception("Key already exists in this project."));

            var result = await _sut.CreateKey(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("Key already exists in this project.", bad.Value);
        }

        [TestMethod]
        public async Task CreateKey_PassesEmpIdFromClaimsToService()
        {
            var request = new CreateKeyRequest { KeyName = "KEY", OriginalText = "text", ProjectId = 1 };

            _serviceMock.Setup(s => s.CreateKey(request, EmpId))
                        .ReturnsAsync(new object());

            await _sut.CreateKey(request);

            // Verify empId extracted from JWT claim was passed correctly
            _serviceMock.Verify(s => s.CreateKey(request, EmpId), Times.Once);
        }

        [TestMethod]
        public async Task CreateKey_MissingOriginalText_Returns400()
        {
            var request = new CreateKeyRequest { KeyName = "KEY", OriginalText = "", ProjectId = 1 };

            _serviceMock.Setup(s => s.CreateKey(request, EmpId))
                        .ThrowsAsync(new Exception("Original text is required."));

            var result = await _sut.CreateKey(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        // ═════════════════════════════════════════════════════════════════════
        // CreateKeys (bulk)
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task CreateKeys_ValidRequest_Returns200WithResult()
        {
            var request = new CreateKeysRequest
            {
                Keys = new List<CreateKeyItem>
                {
                    new() { KeyName = "KEY_A", OriginalText = "A", ProjectId = 1 },
                    new() { KeyName = "KEY_B", OriginalText = "B", ProjectId = 1 }
                }
            };
            var response = new { message = "Keys created successfully.", keyIds = new[] { 1, 2 } };

            _serviceMock.Setup(s => s.CreateKeys(request, EmpId))
                        .ReturnsAsync(response);

            var result = await _sut.CreateKeys(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task CreateKeys_EmptyList_Returns400()
        {
            var request = new CreateKeysRequest { Keys = new List<CreateKeyItem>() };

            _serviceMock.Setup(s => s.CreateKeys(request, EmpId))
                        .ThrowsAsync(new Exception("At least one key is required."));

            var result = await _sut.CreateKeys(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("At least one key is required.", bad.Value);
        }

        [TestMethod]
        public async Task CreateKeys_PassesEmpIdFromClaimsToService()
        {
            var request = new CreateKeysRequest
            {
                Keys = new List<CreateKeyItem>
                {
                    new() { KeyName = "KEY_X", OriginalText = "X", ProjectId = 1 }
                }
            };

            _serviceMock.Setup(s => s.CreateKeys(request, EmpId))
                        .ReturnsAsync(new object());

            await _sut.CreateKeys(request);

            _serviceMock.Verify(s => s.CreateKeys(request, EmpId), Times.Once);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetAllKeys
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetAllKeys_Returns200WithKeyList()
        {
            var keys = new List<object>
            {
                new { KeyName = "KEY_ONE", OriginalText = "One", ProjectId = 1 },
                new { KeyName = "KEY_TWO", OriginalText = "Two", ProjectId = 2 }
            };

            _serviceMock.Setup(s => s.GetAllKeys()).ReturnsAsync(keys);

            var result = await _sut.GetAllKeys();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(keys, ok.Value);
        }

        [TestMethod]
        public async Task GetAllKeys_EmptyResult_Returns200WithEmptyList()
        {
            _serviceMock.Setup(s => s.GetAllKeys())
                        .ReturnsAsync(new List<object>());

            var result = await _sut.GetAllKeys();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task GetAllKeys_CallsServiceExactlyOnce()
        {
            _serviceMock.Setup(s => s.GetAllKeys()).ReturnsAsync(new object());

            await _sut.GetAllKeys();

            _serviceMock.Verify(s => s.GetAllKeys(), Times.Once);
        }

        // ═════════════════════════════════════════════════════════════════════
        // DeleteKey
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task DeleteKey_ValidId_Returns200WithMessage()
        {
            _serviceMock.Setup(s => s.DeleteKey(1)).Returns(Task.CompletedTask);

            var result = await _sut.DeleteKey(1);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Key deleted successfully.", ok.Value);
        }

        [TestMethod]
        public async Task DeleteKey_NotFound_Returns400WithMessage()
        {
            _serviceMock.Setup(s => s.DeleteKey(99))
                        .ThrowsAsync(new Exception("Key not found."));

            var result = await _sut.DeleteKey(99);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("Key not found.", bad.Value);
        }

        [TestMethod]
        public async Task DeleteKey_InvalidId_Returns400()
        {
            _serviceMock.Setup(s => s.DeleteKey(0))
                        .ThrowsAsync(new Exception("Invalid key id."));

            var result = await _sut.DeleteKey(0);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task DeleteKey_CallsServiceWithCorrectId()
        {
            _serviceMock.Setup(s => s.DeleteKey(5)).Returns(Task.CompletedTask);

            await _sut.DeleteKey(5);

            _serviceMock.Verify(s => s.DeleteKey(5), Times.Once);
        }
    }
}