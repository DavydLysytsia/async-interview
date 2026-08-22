using AsyncInterview.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AsyncInterview.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<CandidateProfile> Profiles => Set<CandidateProfile>();
    public DbSet<InterviewQuestion> Questions => Set<InterviewQuestion>();
    public DbSet<VideoResponse> Responses => Set<VideoResponse>();
    public DbSet<YouTubeToken> YouTubeTokens => Set<YouTubeToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.GoogleSubject).IsUnique();

        modelBuilder.Entity<CandidateProfile>()
            .HasIndex(p => p.UserId).IsUnique();
        modelBuilder.Entity<CandidateProfile>()
            .HasOne(p => p.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<CandidateProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One response per user per question — "replace" overwrites it.
        modelBuilder.Entity<VideoResponse>()
            .HasIndex(r => new { r.UserId, r.QuestionId }).IsUnique();
        modelBuilder.Entity<VideoResponse>()
            .Property(r => r.Status).HasConversion<string>();

        modelBuilder.Entity<YouTubeToken>()
            .HasKey(t => t.Key);
    }

    public static void Seed(AppDbContext db)
    {
        if (db.Questions.Any()) return;

        string[] questions =
        {
            "Tell me about yourself.",
            "Why are you interested in this role or field?",
            "Describe a challenging problem you solved and how you approached it.",
            "Tell me about a time you worked in a team. What was your contribution?",
            "What is your greatest professional strength?",
            "What is a weakness you are actively working on?",
            "Describe a project you are proud of.",
            "Where do you see yourself in five years?"
        };

        for (int i = 0; i < questions.Length; i++)
        {
            db.Questions.Add(new InterviewQuestion
            {
                Text = questions[i],
                DisplayOrder = i + 1,
                IsActive = true
            });
        }
        db.SaveChanges();
    }
}
