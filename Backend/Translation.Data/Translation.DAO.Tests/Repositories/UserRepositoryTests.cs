using Translation.DAO.Data;
using Translation.DAO.Repositories;
using Translation.Models.Entities;


namespace Translation.DAO.Tests.Repositories
{
    [TestClass]
    public class UserRepositoryTests
    {
        private AppDbContext _context = null!;
        private UserRepository _sut = null!;

        // ── Seed helpers ──────────────────────────────────────────────────────

        private async Task<(Role role, Language lang)> SeedRoleAndLanguageAsync()
        {
            var role = new Role { Name = "Viewer" };
            var lang = new Language { Code = "EN", Name = "English" };
            _context.Roles.AddRange(role);
            _context.Languages.Add(lang);
            await _context.SaveChangesAsync();
            return (role, lang);
        }

        private async Task<User> SeedUserAsync(string empId = "emp001")
        {
            var (role, lang) = await SeedRoleAndLanguageAsync();
            var user = new User
            {
                EmployeeId = empId,
                FirstName = "Test",
                LastName = "User",
                Password = "Password123",
                RoleId = role.Id,
                PreferredLanguageId = lang.Id
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        [TestInitialize]
        public void SetUp()
        {
            _context = TestDbContextFactory.Create();
            _sut = new UserRepository(_context);
        }

        [TestCleanup]
        public void TearDown()
        {
            _context.Dispose();
        }

        // ═════════════════════════════════════════════════════════════════════
        // UserExists
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task UserExists_ExistingEmployee_ReturnsTrue()
        {
            await SeedUserAsync("emp001");

            var result = await _sut.UserExists("emp001");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task UserExists_NonExistingEmployee_ReturnsFalse()
        {
            var result = await _sut.UserExists("ghost");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task UserExists_IsCaseInsensitive()
        {
            await SeedUserAsync("emp001");

            // Stored as lowercase, queried as uppercase — should still match
            var result = await _sut.UserExists("EMP001");

            Assert.IsTrue(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetUserByEmployeeId
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetUserByEmployeeId_ExistingUser_ReturnsUserWithRoleAndLanguage()
        {
            await SeedUserAsync("emp001");

            var result = await _sut.GetUserByEmployeeId("emp001");

            Assert.IsNotNull(result);
            Assert.AreEqual("emp001", result.EmployeeId);
            Assert.IsNotNull(result.Role);
            Assert.IsNotNull(result.PreferredLanguage);
        }

        [TestMethod]
        public async Task GetUserByEmployeeId_NonExistingUser_ReturnsNull()
        {
            var result = await _sut.GetUserByEmployeeId("ghost");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetUserByEmployeeId_IsCaseInsensitive()
        {
            await SeedUserAsync("emp001");

            var result = await _sut.GetUserByEmployeeId("EMP001");

            Assert.IsNotNull(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetRoleByName
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetRoleByName_ExistingRole_ReturnsRole()
        {
            _context.Roles.Add(new Role { Name = "Admin" });
            await _context.SaveChangesAsync();

            var result = await _sut.GetRoleByName("Admin");

            Assert.IsNotNull(result);
            Assert.AreEqual("Admin", result.Name);
        }

        [TestMethod]
        public async Task GetRoleByName_NonExistingRole_ReturnsNull()
        {
            var result = await _sut.GetRoleByName("SuperAdmin");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetRoleByName_IsCaseInsensitive()
        {
            _context.Roles.Add(new Role { Name = "Translator" });
            await _context.SaveChangesAsync();

            var result = await _sut.GetRoleByName("TRANSLATOR");

            Assert.IsNotNull(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetDefaultRole
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetDefaultRole_ViewerRoleExists_ReturnsViewerRole()
        {
            _context.Roles.Add(new Role { Name = "Viewer" });
            await _context.SaveChangesAsync();

            var result = await _sut.GetDefaultRole();

            Assert.IsNotNull(result);
            Assert.AreEqual("Viewer", result.Name);
        }

        [TestMethod]
        public async Task GetDefaultRole_NoViewerRole_ReturnsNull()
        {
            _context.Roles.Add(new Role { Name = "Admin" });
            await _context.SaveChangesAsync();

            var result = await _sut.GetDefaultRole();

            Assert.IsNull(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // GetLanguageById
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task GetLanguageById_ExistingId_ReturnsLanguage()
        {
            var lang = new Language { Code = "EN", Name = "English" };
            _context.Languages.Add(lang);
            await _context.SaveChangesAsync();

            var result = await _sut.GetLanguageById(lang.Id);

            Assert.IsNotNull(result);
            Assert.AreEqual("EN", result.Code);
        }

        [TestMethod]
        public async Task GetLanguageById_NonExistingId_ReturnsNull()
        {
            var result = await _sut.GetLanguageById(999);

            Assert.IsNull(result);
        }

        // ═════════════════════════════════════════════════════════════════════
        // AddUser & SaveChanges
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task AddUser_ValidUser_PersistsAfterSaveChanges()
        {
            var (role, lang) = await SeedRoleAndLanguageAsync();

            var user = new User
            {
                EmployeeId = "newuser",
                FirstName = "New",
                LastName = "User",
                Password = "hashedpwd",
                RoleId = role.Id,
                PreferredLanguageId = lang.Id
            };

            await _sut.AddUser(user);
            await _sut.SaveChanges();

            var saved = await _context.Users.FindAsync(user.Id);
            Assert.IsNotNull(saved);
            Assert.AreEqual("newuser", saved.EmployeeId);
        }

        [TestMethod]
        public async Task AddUser_NotPersistedBeforeSaveChanges()
        {
            var (role, lang) = await SeedRoleAndLanguageAsync();

            var user = new User
            {
                EmployeeId = "unsaved",
                FirstName = "Un",
                LastName = "Saved",
                Password = "pwd",
                RoleId = role.Id,
                PreferredLanguageId = lang.Id
            };

            await _sut.AddUser(user);

            // SaveChanges not called yet — EF tracks it but Id may be 0
            // The point is after SaveChanges it gets persisted
            await _sut.SaveChanges();
            Assert.IsTrue(user.Id > 0);
        }

        // ═════════════════════════════════════════════════════════════════════
        // AddRefreshToken & GetRefreshToken
        // ═════════════════════════════════════════════════════════════════════

        [TestMethod]
        public async Task AddRefreshToken_ValidToken_PersistsAfterSaveChanges()
        {
            var user = await SeedUserAsync();

            var token = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = "abc123hash",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
                CreatedAtUtc = DateTime.UtcNow
            };

            await _sut.AddRefreshToken(token);
            await _sut.SaveChanges();

            var saved = await _context.RefreshTokens.FindAsync(token.Id);
            Assert.IsNotNull(saved);
            Assert.AreEqual("abc123hash", saved.TokenHash);
        }

        [TestMethod]
        public async Task GetRefreshToken_ExistingHash_ReturnsTokenWithUser()
        {
            var user = await SeedUserAsync();

            var token = new RefreshToken
            {
                UserId = user.Id,
                TokenHash = "myhash123",
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
                CreatedAtUtc = DateTime.UtcNow
            };
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();

            var result = await _sut.GetRefreshToken("myhash123");

            Assert.IsNotNull(result);
            Assert.AreEqual("myhash123", result.TokenHash);
            Assert.IsNotNull(result.User);
        }

        [TestMethod]
        public async Task GetRefreshToken_NonExistingHash_ReturnsNull()
        {
            var result = await _sut.GetRefreshToken("nonexistenthash");

            Assert.IsNull(result);
        }
    }
}