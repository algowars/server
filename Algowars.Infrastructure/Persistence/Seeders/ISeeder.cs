namespace Algowars.Infrastructure.Persistence.Seeders;

internal interface ISeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
