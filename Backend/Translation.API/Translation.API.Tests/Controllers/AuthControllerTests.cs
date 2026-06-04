using Microsoft.AspNetCore.Mvc;
using Moq;
using Translation.API.Controllers;
using Translation.Contracts.DTO.Auth;
using Translation.Service.Interfaces;

namespace Translation.API.Tests.Controllers
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<IAuthService> _serviceMock = null!;
        private AuthController _sut = null!;

        [TestInitialize]
        public void SetUp()
        {
            _serviceMock = new Mock<IAuthService>();
            _sut = new AuthController(_serviceMock.Object);
            ControllerTestHelper.SetUserWithIp(_sut);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Register
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task Register_ValidRequest_Returns200WithMessage()
        {
            var request = new RegisterRequest
            {
                EmployeeId = "emp001",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe",
                PreferredLanguageId = 1
            };

            _serviceMock.Setup(s => s.Register(request))
                        .ReturnsAsync("User registered successfully");

            var result = await _sut.Register(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("User registered successfully", ok.Value);
        }

        [TestMethod]
        public async Task Register_ServiceThrows_Returns400WithMessage()
        {
            var request = new RegisterRequest { EmployeeId = "emp001", Password = "pwd" };

            _serviceMock.Setup(s => s.Register(request))
                        .ThrowsAsync(new Exception("Employee already exists"));

            var result = await _sut.Register(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("Employee already exists", bad.Value);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Login
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task Login_ValidCredentials_Returns200WithTokens()
        {
            var request = new LoginRequest { EmployeeId = "emp001", Password = "Password123" };
            var response = new AuthResponse
            {
                AccessToken = "access-token-123",
                RefreshToken = "refresh-token-456",
                AccessTokenExpiresAtUtc = DateTime.UtcNow.AddHours(1)
            };

            _serviceMock.Setup(s => s.Login(request)).ReturnsAsync(response);

            var result = await _sut.Login(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(response, ok.Value);
        }

        [TestMethod]
        public async Task Login_InvalidCredentials_Returns401WithMessage()
        {
            var request = new LoginRequest { EmployeeId = "emp001", Password = "wrong" };

            _serviceMock.Setup(s => s.Login(request))
                        .ThrowsAsync(new Exception("Invalid password"));

            var result = await _sut.Login(request);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauth = (UnauthorizedObjectResult)result;
            Assert.AreEqual("Invalid password", unauth.Value);
        }

        [TestMethod]
        public async Task Login_UserNotFound_Returns401()
        {
            var request = new LoginRequest { EmployeeId = "ghost", Password = "pwd" };

            _serviceMock.Setup(s => s.Login(request))
                        .ThrowsAsync(new Exception("Invalid employee ID"));

            var result = await _sut.Login(request);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        // ═════════════════════════════════════════════════════════════════════
        // AssignRole
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task AssignRole_ValidRequest_Returns200()
        {
            var request = new AssignRoleRequest { EmployeeId = "emp001", RoleName = "Translator" };
            var response = new { message = "Role assigned successfully", EmployeeId = "emp001", Name = "Translator" };

            _serviceMock.Setup(s => s.AssignRole(request))
                        .ReturnsAsync(response);

            var result = await _sut.AssignRole(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task AssignRole_UserNotFound_Returns400()
        {
            var request = new AssignRoleRequest { EmployeeId = "ghost", RoleName = "Admin" };

            _serviceMock.Setup(s => s.AssignRole(request))
                        .ThrowsAsync(new Exception("User not found"));

            var result = await _sut.AssignRole(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("User not found", bad.Value);
        }

        [TestMethod]
        public async Task AssignRole_RoleNotFound_Returns400()
        {
            var request = new AssignRoleRequest { EmployeeId = "emp001", RoleName = "SuperAdmin" };

            _serviceMock.Setup(s => s.AssignRole(request))
                        .ThrowsAsync(new Exception("Role not found"));

            var result = await _sut.AssignRole(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        // ═════════════════════════════════════════════════════════════════════
        // Refresh
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task Refresh_ValidToken_Returns200WithNewTokens()
        {
            var request = new RefreshRequest { RefreshToken = "valid-refresh-token" };
            var response = new AuthResponse
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token"
            };

            _serviceMock.Setup(s => s.Refresh(request, It.IsAny<string>()))
                        .ReturnsAsync(response);

            var result = await _sut.Refresh(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual(response, ok.Value);
        }

        [TestMethod]
        public async Task Refresh_ExpiredToken_Returns401()
        {
            var request = new RefreshRequest { RefreshToken = "expired-token" };

            _serviceMock.Setup(s => s.Refresh(request, It.IsAny<string>()))
                        .ThrowsAsync(new Exception("Invalid token"));

            var result = await _sut.Refresh(request);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task Refresh_PassesRemoteIpToService()
        {
            var request = new RefreshRequest { RefreshToken = "some-token" };

            _serviceMock.Setup(s => s.Refresh(request, "127.0.0.1"))
                        .ReturnsAsync(new AuthResponse());

            var result = await _sut.Refresh(request);

            // Verify IP was passed — mock only matches on "127.0.0.1"
            _serviceMock.Verify(s => s.Refresh(request, "127.0.0.1"), Times.Once);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Revoke
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task Revoke_ValidToken_Returns200WithMessage()
        {
            var request = new RefreshRequest { RefreshToken = "valid-refresh-token" };

            _serviceMock.Setup(s => s.Revoke(request))
                        .ReturnsAsync("Token revoked");

            var result = await _sut.Revoke(request);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var ok = (OkObjectResult)result;
            Assert.AreEqual("Token revoked", ok.Value);
        }

        [TestMethod]
        public async Task Revoke_TokenNotFound_Returns400()
        {
            var request = new RefreshRequest { RefreshToken = "nonexistent" };

            _serviceMock.Setup(s => s.Revoke(request))
                        .ThrowsAsync(new Exception("Token not found"));

            var result = await _sut.Revoke(request);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var bad = (BadRequestObjectResult)result;
            Assert.AreEqual("Token not found", bad.Value);
        }
    }
}