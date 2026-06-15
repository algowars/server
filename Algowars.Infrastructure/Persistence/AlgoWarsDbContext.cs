using Algowars.Infrastructure.Persistence.Entities.Languages;
using Algowars.Infrastructure.Persistence.Entities.Problems;
using Algowars.Infrastructure.Persistence.Entities.Submissions;
using Algowars.Infrastructure.Persistence.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence;

public sealed class AlgoWarsDbContext(DbContextOptions<AlgoWarsDbContext> options) : DbContext(options)
{
    public DbSet<UserDataModel> Users { get; set; }
    public DbSet<ProgrammingLanguageDataModel> ProgrammingLanguages { get; set; }
    public DbSet<LanguageVersionDataModel> LanguageVersions { get; set; }
    public DbSet<ProblemDataModel> Problems { get; set; }
    public DbSet<ProblemVersionDataModel> ProblemVersions { get; set; }
    public DbSet<TestCaseDataModel> TestCases { get; set; }
    public DbSet<CodeTemplateDataModel> CodeTemplates { get; set; }
    public DbSet<SubmissionDataModel> Submissions { get; set; }
    public DbSet<SubmissionResultDataModel> SubmissionResults { get; set; }
    public DbSet<SubmissionTestCaseDataModel> SubmissionTestCases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlgoWarsDbContext).Assembly);
    }
}
