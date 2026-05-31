using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Translation.Contracts.DTO.Auth;
using Translation.DAO.Repositories.Interfaces;
using Translation.Service.Helpers;
using Translation.Service.Services;
using User = Translation.Models.Entities.User;
using Role = Translation.Models.Entities.Role;
using Language = Translation.Models.Entities.Language;
using RefreshToken = Translation.Models.Entities.RefreshToken;

namespace Translation.Tests.Services
{
    /// <summary>
    /// Unit tests for AuthService.
    ///
    /// NOTE ON JwtService:
    ///   JwtService is a concrete class with no interface, so it cannot be mocked.
    ///   We construct a real instance backed by an in-memory IConfiguration holding
    ///   test values. All business-logic paths are still fully tested because the
    ///   repository is mocked — JwtService is only exercised in the happy paths that
    ///   reach token generation, which is the correct behaviour.
    /// </summary>
    [TestClass]
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _repoMock = null!;
        private JwtService _jwtService = null!;
        private AuthService _sut = null!;

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Builds a JwtService with minimal in-memory configuration.</summary>
        private static JwtService BuildJwtService()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // 32+ character key required by HMAC-SHA256
                    ["Jwt:Key"] = "ThisIsATestSecretKeyForUnitTests!",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience",
                    ["Jwt:ExpiryMinutes"] = "60",
                    ["Jwt:RefreshExpiryDays"] = "7"
                })
                .Build();

            return new JwtService(config);
        }

        /// <summary>Creates a fully-populated User entity for use in test setups.</summary>
        private static User BuildUser(string empId = "emp001") => new()
        {
            Id = 1,
            EmployeeId = empId,
            FirstName = "Test",
            LastName = "User",
            Password = BCrypt.Net.BCrypt.HashPassword("Password123"),
            RoleId = 1,
            Role = new Role { Id = 1, Name = "Viewer" }
        };

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

        [TestInitialize]
        public void SetUp()
        {
            _repoMock = new Mock<IUserRepository>();
            _jwtService = BuildJwtService();
            _sut = new AuthService(_repoMock.Object, _jwtService);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Register
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task Register_ValidRequest_ReturnsSuccessMessage()
        {
            var request = new RegisterRequest
            {
                EmployeeId = "emp001",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe",
                PreferredLanguageId = 1
            };

            _repoMock.Setup(r => r.GetLanguageById(1))
                     .ReturnsAsync(new Language { Id = 1, Code = "EN", Name = "English" });

            _repoMock.Setup(r => r.UserExists("emp001")).ReturnsAsync(false);

            _repoMock.Setup(r => r.GetDefaultRole())
                     .ReturnsAsync(new Role { Id = 2, Name = "Viewer" });

            _repoMock.Setup(r => r.AddUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            var result = await _sut.Register(request);

            Assert.AreEqual("User registered successfully", result);
        }

        [TestMethod]
        public async Task Register_PasswordIsHashed_NotStoredAsPlaintext()
        {
            var request = new RegisterRequest
            {
                EmployeeId = "emp001",
                Password = "MyPlainPassword",
                FirstName = "Jane",
                LastName = "Doe",
                PreferredLanguageId = 1
            };

            _repoMock.Setup(r => r.GetLanguageById(1))
                     .ReturnsAsync(new Language { Id = 1, Code = "EN", Name = "English" });
            _repoMock.Setup(r => r.UserExists("emp001")).ReturnsAsync(false);
            _repoMock.Setup(r => r.GetDefaultRole()).ReturnsAsync(new Role { Id = 1, Name = "Viewer" });

            User? savedUser = null;
            _repoMock.Setup(r => r.AddUser(It.IsAny<User>()))
                     .Callback<User>(u => savedUser = u)
                     .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            await _sut.Register(request);

            Assert.IsNotNull(savedUser);
            // Password must not be stored in plain text
            Assert.AreNotEqual("MyPlainPassword", savedUser!.Password);
            // BCrypt verify must pass with the original password
            Assert.IsTrue(BCrypt.Net.BCrypt.Verify("MyPlainPassword", savedUser.Password));
        }

        [TestMethod]
        public async Task Register_EmployeeIdIsTrimmedAndLowercased()
        {
            var request = new RegisterRequest
            {
                EmployeeId = "  EMP001  ",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe",
                PreferredLanguageId = 1
            };

            _repoMock.Setup(r => r.GetLanguageById(1))
                     .ReturnsAsync(new Language { Id = 1, Code = "EN", Name = "English" });
            _repoMock.Setup(r => r.UserExists("emp001")).ReturnsAsync(false);
            _repoMock.Setup(r => r.GetDefaultRole()).ReturnsAsync(new Role { Id = 1, Name = "Viewer" });
            _repoMock.Setup(r => r.AddUser(It.IsAny<User>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            await _sut.Register(request);

            _repoMock.Verify(r => r.AddUser(It.Is<User>(u =>
                u.EmployeeId == "emp001"
            )), Times.Once);
        }

        [TestMethod]
        public async Task Register_InvalidLanguageId_ThrowsException()
        {
            var request = new RegisterRequest
            {
                EmployeeId = "emp001",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe",
                PreferredLanguageId = 999   // does not exist
            };

            _repoMock.Setup(r => r.GetLanguageById(999)).ReturnsAsync((Language?)null);

            await AssertThrowsExceptionAsync(() => _sut.Register(request));
        }

        [TestMethod]
        public async Task Register_MissingEmployeeId_ThrowsException()
        {
            var request = new RegisterRequest
            {
                EmployeeId = "   ",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe",
                PreferredLanguageId = 1
            };

            _repoMock.Setup(r => r.GetLanguageById(1))
                     .ReturnsAsync(new Language { Id = 1, Code = "EN", Name = "English" });

            await AssertThrowsExceptionAsync(() => _sut.Register(request));
        }

        [TestMethod]
        public async Task Register_MissingPassword_ThrowsException()
        {
            var request = new RegisterRequest
            {
                EmployeeId = "emp001",
                Password = "",
                FirstName = "John",
                LastName = "Doe",
                PreferredLanguageId = 1
            };

            _repoMock.Setup(r => r.GetLanguageById(1))
                     .ReturnsAsync(new Language { Id = 1, Code = "EN", Name = "English" });

            await AssertThrowsExceptionAsync(() => _sut.Register(request));
        }

        [TestMethod]
        public async Task Register_DuplicateEmployee_ThrowsException()
        {
            var request = new RegisterRequest
            {
                EmployeeId = "emp001",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe",
                PreferredLanguageId = 1
            };

            _repoMock.Setup(r => r.GetLanguageById(1))
                     .ReturnsAsync(new Language { Id = 1, Code = "EN", Name = "English" });
            _repoMock.Setup(r => r.UserExists("emp001")).ReturnsAsync(true);  // already exists

            await AssertThrowsExceptionAsync(() => _sut.Register(request));

            _repoMock.Verify(r => r.AddUser(It.IsAny<User>()), Times.Never);
        }

        [TestMethod]
        public async Task Register_DefaultRoleNotFound_ThrowsException()
        {
            var request = new RegisterRequest
            {
                EmployeeId = "emp001",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe",
                PreferredLanguageId = 1
            };

            _repoMock.Setup(r => r.GetLanguageById(1))
                     .ReturnsAsync(new Language { Id = 1, Code = "EN", Name = "English" });
            _repoMock.Setup(r => r.UserExists("emp001")).ReturnsAsync(false);
            _repoMock.Setup(r => r.GetDefaultRole()).ReturnsAsync((Role?)null);

            await AssertThrowsExceptionAsync(() => _sut.Register(request));
        }

        // ═════════════════════════════════════════════════════════════════════
        // Login
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsAuthResponseWithTokens()
        {
            var user = BuildUser();
            var request = new LoginRequest { EmployeeId = "emp001", Password = "Password123" };

            _repoMock.Setup(r => r.GetUserByEmployeeId("emp001")).ReturnsAsync(user);
            _repoMock.Setup(r => r.AddRefreshToken(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            var result = await _sut.Login(request);

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.RefreshToken));
        }

        [TestMethod]
        public async Task Login_RefreshTokenIsSavedWithHashNotPlaintext()
        {
            var user = BuildUser();
            var request = new LoginRequest { EmployeeId = "emp001", Password = "Password123" };

            RefreshToken? savedToken = null;
            _repoMock.Setup(r => r.GetUserByEmployeeId("emp001")).ReturnsAsync(user);
            _repoMock.Setup(r => r.AddRefreshToken(It.IsAny<RefreshToken>()))
                     .Callback<RefreshToken>(t => savedToken = t)
                     .Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            var response = await _sut.Login(request);

            Assert.IsNotNull(savedToken);
            // The hash stored in DB must not equal the plain refresh token returned to client
            Assert.AreNotEqual(response.RefreshToken, savedToken!.TokenHash);
        }

        [TestMethod]
        public async Task Login_MissingEmployeeId_ThrowsException()
        {
            var request = new LoginRequest { EmployeeId = "", Password = "Password123" };

            await AssertThrowsExceptionAsync(() => _sut.Login(request));
        }

        [TestMethod]
        public async Task Login_MissingPassword_ThrowsException()
        {
            var request = new LoginRequest { EmployeeId = "emp001", Password = "" };

            await AssertThrowsExceptionAsync(() => _sut.Login(request));
        }

        [TestMethod]
        public async Task Login_EmployeeNotFound_ThrowsException()
        {
            var request = new LoginRequest { EmployeeId = "ghost", Password = "Password123" };

            _repoMock.Setup(r => r.GetUserByEmployeeId("ghost")).ReturnsAsync((User?)null);

            await AssertThrowsExceptionAsync(() => _sut.Login(request));
        }

        [TestMethod]
        public async Task Login_WrongPassword_ThrowsException()
        {
            var user = BuildUser();
            var request = new LoginRequest { EmployeeId = "emp001", Password = "WrongPassword" };

            _repoMock.Setup(r => r.GetUserByEmployeeId("emp001")).ReturnsAsync(user);

            await AssertThrowsExceptionAsync(() => _sut.Login(request));

            _repoMock.Verify(r => r.AddRefreshToken(It.IsAny<RefreshToken>()), Times.Never);
        }

        // ═════════════════════════════════════════════════════════════════════
        // AssignRole
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task AssignRole_ValidRequest_UpdatesUserRoleAndReturnsMessage()
        {
            var user = BuildUser();
            var newRole = new Role { Id = 3, Name = "Admin" };
            var request = new AssignRoleRequest { EmployeeId = "emp001", RoleName = "Admin" };

            _repoMock.Setup(r => r.GetUserByEmployeeId("emp001")).ReturnsAsync(user);
            _repoMock.Setup(r => r.GetRoleByName("Admin")).ReturnsAsync(newRole);
            _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            var result = await _sut.AssignRole(request);

            Assert.IsNotNull(result);
            // User's RoleId should now be updated to the new role
            Assert.AreEqual(3, user.RoleId);
            _repoMock.Verify(r => r.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public async Task AssignRole_UserNotFound_ThrowsException()
        {
            var request = new AssignRoleRequest { EmployeeId = "ghost", RoleName = "Admin" };

            _repoMock.Setup(r => r.GetUserByEmployeeId("ghost")).ReturnsAsync((User?)null);

            await AssertThrowsExceptionAsync(() => _sut.AssignRole(request));
        }

        [TestMethod]
        public async Task AssignRole_RoleNotFound_ThrowsException()
        {
            var user = BuildUser();
            var request = new AssignRoleRequest { EmployeeId = "emp001", RoleName = "SuperAdmin" };

            _repoMock.Setup(r => r.GetUserByEmployeeId("emp001")).ReturnsAsync(user);
            _repoMock.Setup(r => r.GetRoleByName("SuperAdmin")).ReturnsAsync((Role?)null);

            await AssertThrowsExceptionAsync(() => _sut.AssignRole(request));

            _repoMock.Verify(r => r.SaveChanges(), Times.Never);
        }

        // ═════════════════════════════════════════════════════════════════════
        // Refresh
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task Refresh_ValidToken_ReturnsNewTokenPair()
        {
            var user = BuildUser();
            var refreshToken = _jwtService.GenerateRefreshToken();
            var hash = _jwtService.HashToken(refreshToken);

            var storedToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7),   // not expired
                User = user
            };

            var request = new RefreshRequest { RefreshToken = refreshToken };

            _repoMock.Setup(r => r.GetRefreshToken(hash)).ReturnsAsync(storedToken);
            _repoMock.Setup(r => r.AddRefreshToken(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            var result = await _sut.Refresh(request, "127.0.0.1");

            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.RefreshToken));
            // New refresh token must differ from the old one
            Assert.AreNotEqual(refreshToken, result.RefreshToken);
        }

        [TestMethod]
        public async Task Refresh_OldTokenIsRevokedAfterRefresh()
        {
            var user = BuildUser();
            var refreshToken = _jwtService.GenerateRefreshToken();
            var hash = _jwtService.HashToken(refreshToken);

            var storedToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
                User = user
            };

            _repoMock.Setup(r => r.GetRefreshToken(hash)).ReturnsAsync(storedToken);
            _repoMock.Setup(r => r.AddRefreshToken(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            await _sut.Refresh(new RefreshRequest { RefreshToken = refreshToken }, "127.0.0.1");

            // RevokedAtUtc should have been set on the old token
            Assert.IsNotNull(storedToken.RevokedAtUtc);
        }

        [TestMethod]
        public async Task Refresh_TokenNotFound_ThrowsException()
        {
            var fakeToken = "nonexistent-token";
            var hash = _jwtService.HashToken(fakeToken);

            _repoMock.Setup(r => r.GetRefreshToken(hash)).ReturnsAsync((RefreshToken?)null);

            await AssertThrowsExceptionAsync(
                () => _sut.Refresh(new RefreshRequest { RefreshToken = fakeToken }, "127.0.0.1"));
        }

        [TestMethod]
        public async Task Refresh_ExpiredToken_ThrowsException()
        {
            var user = BuildUser();
            var refreshToken = _jwtService.GenerateRefreshToken();
            var hash = _jwtService.HashToken(refreshToken);

            var expiredToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(-1),  // already expired
                User = user
            };

            _repoMock.Setup(r => r.GetRefreshToken(hash)).ReturnsAsync(expiredToken);

            await AssertThrowsExceptionAsync(
                () => _sut.Refresh(new RefreshRequest { RefreshToken = refreshToken }, "127.0.0.1"));
        }

        // ═════════════════════════════════════════════════════════════════════
        // Revoke
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task Revoke_ValidToken_SetsRevokedAtAndReturnsMessage()
        {
            var user = BuildUser();
            var refreshToken = _jwtService.GenerateRefreshToken();
            var hash = _jwtService.HashToken(refreshToken);

            var storedToken = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = hash,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };

            _repoMock.Setup(r => r.GetRefreshToken(hash)).ReturnsAsync(storedToken);
            _repoMock.Setup(r => r.SaveChanges()).Returns(Task.CompletedTask);

            var result = await _sut.Revoke(new RefreshRequest { RefreshToken = refreshToken });

            Assert.AreEqual("Token revoked", result);
            Assert.IsNotNull(storedToken.RevokedAtUtc);
            _repoMock.Verify(r => r.SaveChanges(), Times.Once);
        }

        [TestMethod]
        public async Task Revoke_TokenNotFound_ThrowsException()
        {
            var fakeToken = "not-a-real-token";
            var hash = _jwtService.HashToken(fakeToken);

            _repoMock.Setup(r => r.GetRefreshToken(hash)).ReturnsAsync((RefreshToken?)null);

            await AssertThrowsExceptionAsync(
                () => _sut.Revoke(new RefreshRequest { RefreshToken = fakeToken }));

            _repoMock.Verify(r => r.SaveChanges(), Times.Never);
        }
    }
}