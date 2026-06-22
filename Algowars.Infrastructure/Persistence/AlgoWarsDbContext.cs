using Algowars.Domain.Languages.Entities;
using Algowars.Domain.Problems.Entities;
using Algowars.Domain.TestSuites.Entities;
using Algowars.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence;

internal sealed class AlgowarsDbContext(DbContextOptions<AlgowarsDbContext> options) : DbContext(options)
{
    public DbSet<Language> Languages { get; init; }
    public DbSet<Problem> Problems { get; init; }
    public DbSet<TestSuite> TestSuites { get; init; }
    public DbSet<User> Users { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlgowarsDbContext).Assembly);
    }
}