using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Algowars.Infrastructure.Persistence;

internal sealed class AlgowarsDbContextFactory : IDesignTimeDbContextFactory<AlgowarsDbContext>
{
    public AlgowarsDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        // Directory.GetCurrentDirectory() resolves to Algowars.Api when run via
        // `dotnet ef --startup-project Algowars.Api`, giving access to its appsettings.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No connection string found. Ensure 'ConnectionStrings:DefaultConnection' is configured.");

        var options = new DbContextOptionsBuilder<AlgowarsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AlgowarsDbContext(options);
    }
}
