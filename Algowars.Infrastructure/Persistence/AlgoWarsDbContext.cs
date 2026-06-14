using Algowars.Infrastructure.Persistence.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence;

internal sealed class AlgoWarsDbContext(DbContextOptions<AlgoWarsDbContext> options) : DbContext(options)
{
    public DbSet<UserDataModel> Users => Set<UserDataModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDataModel>()
            .HasIndex(u => u.Sub).IsUnique();

        modelBuilder.Entity<UserDataModel>()
            .HasIndex(u => u.Username).IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
