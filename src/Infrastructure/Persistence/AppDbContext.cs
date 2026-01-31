using ApplicationCore.Domain.Problems.TestSuites;
using Infrastructure.Persistence.Entities.Account;
using Infrastructure.Persistence.Entities.Language;
using Infrastructure.Persistence.Entities.Problem;
using Infrastructure.Persistence.Entities.Submission;
using Infrastructure.Persistence.Entities.Submission.Outbox;
using Infrastructure.Persistence.Entities.TestSuite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AccountEntity> Accounts { get; set; }

    public DbSet<HarnessTemplateEntity> HarnessTemplates { get; set; }

    public DbSet<LanguageVersionEntity> LanguageVersions { get; set; }

    public DbSet<ProblemEntity> Problems { get; set; }

    public DbSet<ProblemHistoryEntity> ProblemHistories { get; set; }

    public DbSet<ProblemSetupEntity> ProblemSetups { get; set; }

    public DbSet<ProblemStatusEntity> ProblemStatuses { get; set; }

    public DbSet<ProgrammingLanguageEntity> ProgrammingLanguages { get; set; }

    public DbSet<SubmissionOutboxEntity> SubmissionOutbox { get; set; }

    public DbSet<SubmissionOutboxStatusEntity> SubmissionOutboxStatuses { get; set; }

    public DbSet<SubmissionOutboxTypeEntity> SubmissionOutboxTypes { get; set; }

    public DbSet<SubmissionEntity> Submissions { get; set; }

    public DbSet<SubmissionResultEntity> SubmissionResults { get; set; }

    public DbSet<SubmissionStatusEntity> SubmissionStatuses { get; set; }

    public DbSet<SubmissionStatusTypeEntity> SubmissionStatusTypes { get; set; }

    public DbSet<TagEntity> Tags { get; set; }

    public DbSet<TestCaseEntity> TestCases { get; set; }

    public DbSet<TestCaseIoPayloadEntity> TestCaseIoPayloads { get; set; }

    public DbSet<TestCaseTypeEntity> TestCaseTypes { get; set; }

    public DbSet<TestSuiteEntity> TestSuites { get; set; }

    public DbSet<TestSuiteTypeEntity> TestSuiteTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ModelProblems(modelBuilder);
        ModelProblemSetupsTestSuites(modelBuilder);
    }

    private static void ModelProblems(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ProblemEntity>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Problems)
            .UsingEntity<Dictionary<string, object>>(
                "problem_tags",
                j => j.HasOne<TagEntity>().WithMany().HasForeignKey("tag_id"),
                j => j.HasOne<ProblemEntity>().WithMany().HasForeignKey("problem_id")
            );
    }

    private static void ModelProblemSetupsTestSuites(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ProblemSetupEntity>()
            .HasMany(ps => ps.TestSuites)
            .WithMany(ts => ts.Setups)
            .UsingEntity<Dictionary<string, object>>(
                "problem_setup_test_suites",
                j =>
                    j.HasOne<TestSuiteEntity>()
                        .WithMany()
                        .HasForeignKey("test_suite_id")
                        .HasConstraintName("fk_problem_setup_test_suites_test_suite_id"),
                j =>
                    j.HasOne<ProblemSetupEntity>()
                        .WithMany()
                        .HasForeignKey("problem_setup_id")
                        .HasConstraintName("fk_problem_setup_test_suites_problem_setup_id"),
                j =>
                {
                    j.ToTable("problem_setup_test_suites");
                    j.HasKey("problem_setup_id", "test_suite_id");
                }
            );
    }
}
