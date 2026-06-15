using Algowars.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Algowars.Infrastructure;

public sealed class AlgoWarsDbContextFactory : IDesignTimeDbContextFactory<AlgoWarsDbContext>
{
    public AlgoWarsDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__algowars-db")
            ?? "Host=localhost;Port=5432;Database=algowars;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AlgoWarsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AlgoWarsDbContext(options);
    }
}
