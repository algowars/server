using Algowars.Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

internal sealed class AlgowarsDbContext(DbContextOptions<AlgowarsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlgowarsDbContext).Assembly);
    }
}
