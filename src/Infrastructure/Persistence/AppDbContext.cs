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

    public DbSet<LanguageVersionEngineMappingEntity> LanguageVersionEngineMappings { get; set; }

    public DbSet<ProblemEntity> Problems { get; set; }

    public DbSet<ProblemHistoryEntity> ProblemHistories { get; set; }

    public DbSet<ProblemSetupEntity> ProblemSetups { get; set; }

    public DbSet<ProblemStatusEntity> ProblemStatuses { get; set; }

    public DbSet<ProgrammingLanguageEntity> ProgrammingLanguages { get; set; }

    public DbSet<SubmissionOutboxEntity> SubmissionOutboxes { get; set; }

    public DbSet<SubmissionOutboxStatusEntity> SubmissionOutboxStatuses { get; set; }

    public DbSet<SubmissionOutboxTypeEntity> SubmissionOutboxTypes { get; set; }

    public DbSet<SubmissionEntity> Submissions { get; set; }

    public DbSet<SubmissionResultEntity> SubmissionResults { get; set; }

    public DbSet<SubmissionStatusEntity> SubmissionStatuses { get; set; }

    public DbSet<SubmissionStatusTypeEntity> SubmissionStatusTypes { get; set; }

    public DbSet<TagEntity> Tags { get; set; }

    public DbSet<TestCaseEntity> TestCases { get; set; }

    public DbSet<TestCaseExpectedOutputEntity> TestCaseExpectedOutputs { get; set; }

    public DbSet<TestCasesInputsValueTypeEntity> TestCasesInputsValueTypes { get; set; }

    public DbSet<TestCasesOutputTypeEntity> TestCasesOutputTypes { get; set; }

    public DbSet<TestSuiteEntity> TestSuites { get; set; }

    public DbSet<TestSuiteTypeEntity> TestSuiteTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ModelProblems(modelBuilder);
        ModelProblemSetupsTestSuites(modelBuilder);
        ModelTestSuiteTestCases(modelBuilder);
        ModelTestCaseInputs(modelBuilder);
        ModelTestCaseExpectedOutput(modelBuilder);
        ModelLanguageVersionEngineMappings(modelBuilder);
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
                        .HasConstraintName("fk_problem_setup_test_suites_test_suite"),
                j =>
                    j.HasOne<ProblemSetupEntity>()
                        .WithMany()
                        .HasForeignKey("problem_setup_id")
                        .HasConstraintName("fk_problem_setup_test_suites_problem_setup"),
                j =>
                {
                    j.ToTable("problem_setup_test_suites");
                    j.HasKey("problem_setup_id", "test_suite_id");
                }
            );
    }

    private static void ModelTestSuiteTestCases(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<TestSuiteEntity>()
            .HasMany(ts => ts.TestCases)
            .WithOne(tc => tc.TestSuite)
            .HasForeignKey(tc => tc.TestSuiteId)
            .HasConstraintName("fk_test_cases_test_suite_id");
    }

    private static void ModelTestCaseInputs(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<TestCaseEntity>()
            .HasMany(tc => tc.InputParams)
            .WithOne(i => i.TestCase)
            .HasForeignKey(i => i.TestCaseId)
            .HasConstraintName("fk_test_cases_inputs_test_case_id");
    }

    private static void ModelTestCaseExpectedOutput(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<TestCaseEntity>()
            .HasOne(tc => tc.ExpectedOutput)
            .WithOne(o => o.TestCase)
            .HasForeignKey<TestCaseExpectedOutputEntity>(o => o.TestCaseId)
            .HasConstraintName("fk_test_cases_expected_outputs_test_case_id");

        modelBuilder
            .Entity<TestCaseExpectedOutputEntity>()
            .HasOne(o => o.OutputType)
            .WithMany()
            .HasForeignKey(o => o.OutputValueTypeId)
            .HasConstraintName("fk_test_cases_expected_outputs_output_type");
    }

    private static void ModelLanguageVersionEngineMappings(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<LanguageVersionEntity>()
            .HasMany(lv => lv.EngineMappings)
            .WithOne(m => m.LanguageVersion)
            .HasForeignKey(m => m.ProgrammingLanguageVersionId)
            .HasConstraintName("fk_lang_version_engine_mappings_language_version");
    }
}