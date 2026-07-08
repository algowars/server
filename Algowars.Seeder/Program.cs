using Algowars.Infrastructure;
using Algowars.Infrastructure.Persistence.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

bool seedStatic = args.Contains("--static") || args.Contains("--all");
bool seedDemo = args.Contains("--demo") || args.Contains("--all");

if (!seedStatic && !seedDemo)
{
    Console.WriteLine("Usage: dotnet run --project Algowars.Seeder -- [--static] [--demo] [--all]");
    Console.WriteLine();
    Console.WriteLine("  --static   Seed reference data (languages and versions)");
    Console.WriteLine("  --demo     Seed demo data (example problems and test suites)");
    Console.WriteLine("  --all      Seed everything");
    return 1;
}

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .Build();

ServiceCollection services = new();
services.AddInfrastructure(configuration);

await using ServiceProvider provider = services.BuildServiceProvider();
IServiceProvider sp = provider;

await sp.MigrateAsync();
await sp.SeedAsync(new SeederOptions
{
    SeedStaticData = seedStatic,
    SeedDemoData = seedDemo
});

Console.WriteLine("Done.");
return 0;
