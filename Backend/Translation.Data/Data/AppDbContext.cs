using Microsoft.EntityFrameworkCore;
using Translation.Models.Entities;

namespace Translation.DAO.Data
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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
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
    new Language { Id = 1, Code = "en", Name = "English" },
    new Language { Id = 2, Code = "es", Name = "Spanish" },
    new Language { Id = 3, Code = "fr", Name = "French" },
    new Language { Id = 4, Code = "de", Name = "German" },
    new Language { Id = 5, Code = "ja", Name = "Japanese" }
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
            .HasIndex(t => new { t.TranslationKeyId, t.LanguageCode })
            .IsUnique();

            modelBuilder.Entity<TranslationValue>()
            .HasOne(t => t.TranslationKey)
            .WithMany(k => k.Translations)
            .HasForeignKey(t => t.TranslationKeyId)
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

            modelBuilder.Entity<Language>(b =>
            {
                b.HasKey(l => l.Id);
                b.Property(l => l.Code).IsRequired().HasMaxLength(10);
                b.Property(l => l.Name).IsRequired();
                b.HasIndex(l => l.Code).IsUnique();
            });
            modelBuilder.Entity<TranslationValue>()
            .HasOne(tv => tv.TranslationKey)
            .WithMany(tk => tk.Translations)
            .HasForeignKey(tv => tv.TranslationKeyId);

            modelBuilder.Entity<TranslationValue>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.Value).IsRequired();

                // Configure Relationship from TranslationValue.LanguageCode -> Language.Code
                b.Property(t => t.LanguageCode).IsRequired().HasMaxLength(10);
                b.HasOne(t => t.Language)
                    .WithMany(l => l.TranslationValues)
                    .HasForeignKey(t => t.LanguageCode)
                    .HasPrincipalKey(l => l.Code)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(t => t.TranslationKey)
                    .WithMany(k => k.Translations)
                    .HasForeignKey(t => t.TranslationKeyId)
                    .OnDelete(DeleteBehavior.Cascade);

            });
        }
    }
}
