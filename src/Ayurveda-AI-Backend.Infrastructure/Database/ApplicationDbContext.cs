using Ayurveda_AI_Backend.Domain.Entities;
using Ayurveda_AI_Backend.Domain.Seeds;
using Microsoft.EntityFrameworkCore;

namespace Ayurveda_AI_Backend.Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<PrakritiQuizResponse> PrakritiQuizResponses => Set<PrakritiQuizResponse>();
    public DbSet<PrakritiResult> PrakritiResults => Set<PrakritiResult>();
    public DbSet<HealthSignal> HealthSignals => Set<HealthSignal>();
    public DbSet<ChronicCondition> ChronicConditions => Set<ChronicCondition>();
    public DbSet<UserLifestyleProfile> UserLifestyleProfiles => Set<UserLifestyleProfile>();
    public DbSet<VikritiSnapshot> VikritiSnapshots => Set<VikritiSnapshot>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponRedemption> CouponRedemptions => Set<CouponRedemption>();
    public DbSet<UserUsage> UserUsages => Set<UserUsage>();
    public DbSet<AccessPolicy> AccessPolicies => Set<AccessPolicy>();
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<McqResponse> McqResponses => Set<McqResponse>();
    public DbSet<HealthIndicator> HealthIndicators => Set<HealthIndicator>();
    public DbSet<PoopType> PoopTypes => Set<PoopType>();
    public DbSet<EnergyLevel> EnergyLevels => Set<EnergyLevel>();
    public DbSet<GeminiQuestion> GeminiQuestions => Set<GeminiQuestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<UserProfile>()
            .HasKey(p => p.UserId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId);

        modelBuilder.Entity<PrakritiQuizResponse>()
            .HasOne(p => p.Question)
            .WithMany()
            .HasForeignKey(p => p.QuestionId);

        modelBuilder.Entity<UserUsage>()
            .HasIndex(u => new { u.UserId, u.Date })
            .IsUnique();

        modelBuilder.Entity<Coupon>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<QuizOption>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId);

        modelBuilder.Entity<PrakritiQuizResponse>()
            .HasOne(r => r.User)
            .WithMany(u => u.PrakritiQuizResponses)
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<McqResponse>()
            .HasOne(r => r.User)
            .WithMany(u => u.McqResponses)
            .HasForeignKey(r => r.UserId);

        modelBuilder.Entity<McqResponse>()
            .HasOne(r => r.Question)
            .WithMany()
            .HasForeignKey(r => r.QuestionId);

        modelBuilder.Entity<PoopType>().HasData(SeedData.PoopTypes);
        modelBuilder.Entity<EnergyLevel>().HasData(SeedData.EnergyLevels);
        modelBuilder.Entity<HealthIndicator>().HasData(SeedData.Indicators);
        modelBuilder.Entity<GeminiQuestion>().HasData(SeedData.GeminiQuestions);
    }
}
