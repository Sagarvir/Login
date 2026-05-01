using login1.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace login1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Language> Languages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(x => x.TokenHash)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Language>().HasData(
    new Language { Id = 1, Name = "English" },
    new Language { Id = 2, Name = "Spanish" },
    new Language { Id = 3, Name = "French" },
    new Language { Id = 4, Name = "German" },
    new Language { Id = 5, Name = "Japanese" }
);

            // Configure User -> Language relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.PreferredLanguage)
                .WithMany()
                .HasForeignKey(u => u.PreferredLanguageId);

            // Configure User -> Role relationship ✅ ADDED
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
