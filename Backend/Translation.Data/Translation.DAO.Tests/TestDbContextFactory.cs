using Microsoft.EntityFrameworkCore;
using Translation.DAO.Data;

namespace Translation.DAO.Tests
{
    /// <summary>
    /// Creates a fresh in-memory AppDbContext for each test.
    /// Each test gets its own isolated database — no shared state between tests.
    /// </summary>
    public static class TestDbContextFactory
    {
        public static AppDbContext Create(string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}