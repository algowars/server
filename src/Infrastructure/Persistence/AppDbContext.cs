using ApplicationCore.Domain.Problems.TestSuites;
using Infrastructure.Persistence.Entities.Account;
using Infrastructure.Persistence.Entities.Language;
using Infrastructure.Persistence.Entities.Problem;
using Infrastructure.Persistence.Entities.TestSuite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public interface IAppDbContext
{
    DbSet<AccountEntity> Accounts { get; set; }

    DbSet<HarnessTemplateEntity> HarnessTemplates { get; set; }

    DbSet<LanguageVersionEntity> LanguageVersions { get; set; }

    DbSet<ProblemEntity> Problems { get; set; }

    DbSet<ProblemHistoryEntity> ProblemHistories { get; set; }

    DbSet<ProblemSetupEntity> ProblemSetups { get; set; }

    DbSet<ProblemStatusEntity> ProblemStatuses { get; set; }

    DbSet<ProgrammingLanguageEntity> ProgrammingLanguages { get; set; }

    DbSet<TagEntity> Tags { get; set; }

    DbSet<TestCaseEntity> TestCases { get; set; }

    DbSet<TestCaseIoPayloadEntity> TestCaseIoPayloads { get; set; }

    DbSet<TestCaseTypeEntity> TestCaseTypes { get; set; }

    DbSet<TestSuiteEntity> TestSuites { get; set; }

    DbSet<TestSuiteTypeEntity> TestSuiteTypes { get; set; }

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options),
        IAppDbContext
{
    public DbSet<AccountEntity> Accounts { get; set; }

    public DbSet<HarnessTemplateEntity> HarnessTemplates { get; set; }

    public DbSet<LanguageVersionEntity> LanguageVersions { get; set; }

    public DbSet<ProblemEntity> Problems { get; set; }

    public DbSet<ProblemHistoryEntity> ProblemHistories { get; set; }

    public DbSet<ProblemSetupEntity> ProblemSetups { get; set; }

    public DbSet<ProblemStatusEntity> ProblemStatuses { get; set; }

    public DbSet<ProgrammingLanguageEntity> ProgrammingLanguages { get; set; }

    public DbSet<TagEntity> Tags { get; set; }

    public DbSet<TestCaseEntity> TestCases { get; set; }

    public DbSet<TestCaseIoPayloadEntity> TestCaseIoPayloads { get; set; }

    public DbSet<TestCaseTypeEntity> TestCaseTypes { get; set; }

    public DbSet<TestSuiteEntity> TestSuites { get; set; }

    public DbSet<TestSuiteTypeEntity> TestSuiteTypes { get; set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
