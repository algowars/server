using Microsoft.EntityFrameworkCore;

namespace Algowars.Infrastructure.Persistence.Seeders;

/// <summary>
/// Base class for seeds that should only ever run once.
/// The seed name is stored in <c>seed_history</c> after first execution;
/// subsequent runs are silently skipped.
/// </summary>
internal abstract class OnceSeeder(AlgowarsDbContext context) : ISeeder
{
    /// <summary>
    /// Stable, unique name for this seed (e.g. "20260712_SeedInitialProblems").
    /// Never rename this after it has run in any environment.
    /// </summary>
    public abstract string Name { get; }

    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        bool alreadyRun = await context.SeedHistory
            .AnyAsync(e => e.Name == Name, cancellationToken);

        if (alreadyRun)
        {
            Console.WriteLine($"[seed] Skipping '{Name}' (already run).");
            return;
        }

        Console.WriteLine($"[seed] Running '{Name}'...");
        await ExecuteAsync(cancellationToken);

        context.SeedHistory.Add(new SeedHistoryEntry(Name));
        await context.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"[seed] '{Name}' complete.");
    }
}
