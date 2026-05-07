using Backend_API_s.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // 🔹 DbSets (Tables)
    public DbSet<User> Users { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<TranslationKey> TranslationKeys { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<KeyTag> KeyTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------------- USER ----------------
        modelBuilder.Entity<User>()
            .HasKey(u => u.UserId);

        modelBuilder.Entity<User>()
            .Property(u => u.PasswordHash)
            .IsRequired();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasOne(u => u.PreferredLanguage)
            .WithMany(l => l.Users)
            .HasForeignKey(u => u.PreferredLanguageCode)
            .OnDelete(DeleteBehavior.Restrict);

        // ---------------- LANGUAGE ----------------
        modelBuilder.Entity<Language>()
            .HasKey(l => l.Code);

        modelBuilder.Entity<Language>()
            .Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);

        // ---------------- TRANSLATION KEY ----------------
        modelBuilder.Entity<TranslationKey>()
            .HasKey(k => k.Id);

        modelBuilder.Entity<TranslationKey>()
            .Property(k => k.Key)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<TranslationKey>()
            .HasIndex(k => k.Key)
            .IsUnique(); // 🔥 Important

        modelBuilder.Entity<TranslationKey>()
            .Property(k => k.DefaultText)
            .IsRequired();

        // ---------------- TRANSLATION ----------------
        modelBuilder.Entity<Translation>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<Translation>()
            .Property(t => t.TranslatedText)
            .IsRequired();

        modelBuilder.Entity<Translation>()
            .HasOne(t => t.TranslationKey)
            .WithMany(k => k.Translations)
            .HasForeignKey(t => t.TranslationKeyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Translation>()
            .HasOne(t => t.Language)
            .WithMany(l => l.Translations)
            .HasForeignKey(t => t.LanguageCode)
            .OnDelete(DeleteBehavior.Restrict);

        // 🔥 Prevent duplicate translations per language per key
        modelBuilder.Entity<Translation>()
            .HasIndex(t => new { t.TranslationKeyId, t.LanguageCode })
            .IsUnique();

        // ---------------- TAG ----------------
        modelBuilder.Entity<Tag>()
            .HasKey(t => t.TagId);

        modelBuilder.Entity<Tag>()
            .Property(t => t.TagName)
            .IsRequired()
            .HasMaxLength(100);

        // ---------------- KEY TAG (Many-to-Many) ----------------
        modelBuilder.Entity<KeyTag>()
            .HasKey(kt => new { kt.TranslationKeyId, kt.TagId });

        modelBuilder.Entity<KeyTag>()
            .HasOne(kt => kt.TranslationKey)
            .WithMany(k => k.KeyTags)
            .HasForeignKey(kt => kt.TranslationKeyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<KeyTag>()
            .HasOne(kt => kt.Tag)
            .WithMany(t => t.KeyTags)
            .HasForeignKey(kt => kt.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}