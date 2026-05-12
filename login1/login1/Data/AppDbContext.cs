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
        public DbSet<TranslationKey> TranslationKeys { get; set; }
        public DbSet<TranslationValue> TranslationValues { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<KeyProject> KeyProjects { get; set; }
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
            modelBuilder.Entity<TranslationKey>()
            .HasIndex(k => k.KeyName)
            .IsUnique();

            modelBuilder.Entity<TranslationValue>()
            .HasIndex(t => new { t.KeyId, t.LanguageCode })
            .IsUnique();

            modelBuilder.Entity<TranslationValue>()
            .HasOne(t => t.Key)
            .WithMany(k => k.Translations)
            .HasForeignKey(t => t.KeyId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KeyProject>()
            .HasOne(kp => kp.Key)
            .WithMany(k => k.KeyProjects)
            .HasForeignKey(kp => kp.KeyId);

            modelBuilder.Entity<KeyProject>()
            .HasOne(kp => kp.Project)
            .WithMany(p => p.KeyProjects)
            .HasForeignKey(kp => kp.ProjectId);

            modelBuilder.Entity<KeyProject>()
            .HasIndex(kp => new { kp.KeyId, kp.ProjectId })
            .IsUnique();
        }
    }
}
