using Algowars.Domain.ExecutionPipelines;
using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Languages.Enums;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.Problems.Enums;
using Algowars.Domain.Submissions.Entities;
using Algowars.Domain.SubmissionJobs;
using Algowars.Domain.TestSuites.Entities;
using Algowars.Domain.Users.Entities;
using Algowars.Infrastructure.ExecutionEngine.Assert;
using Algowars.Infrastructure.ExecutionEngine.Judge0;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence;

internal sealed class AlgowarsDbContext(DbContextOptions<AlgowarsDbContext> options) : DbContext(options)
{
    public DbSet<ExecutionPipeline> ExecutionPipelines { get; init; }

    public DbSet<Language> Languages { get; init; }

    public DbSet<Problem> Problems { get; init; }

    public DbSet<ProblemTag> Tags { get; init; }

    public DbSet<Submission> Submissions { get; init; }

    public DbSet<SubmissionJob> SubmissionJobs { get; init; }

    public DbSet<TestSuite> TestSuites { get; init; }

    public DbSet<User> Users { get; init; }

    // Infrastructure-only — not domain aggregates
    public DbSet<Judge0StepConfiguration> Judge0StepConfigurations { get; init; }
    public DbSet<AssertStepConfiguration> AssertStepConfigurations { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlgowarsDbContext).Assembly);

        modelBuilder.Entity<Problem>().HasQueryFilter(problem => problem.Status == ProblemStatus.Published);
        modelBuilder.Entity<Language>().HasQueryFilter(language => language.Status == LanguageStatus.Active);
        modelBuilder.Entity<LanguageVersionEntry>().HasQueryFilter(version => version.Status == LanguageVersionStatus.Active);
    }
}
