namespace Algowars.Infrastructure.Persistence.Seeders;

public sealed class SeederOptions
{
    /// <summary>
    /// Seeds reference data (languages and versions). Idempotent — safe to run on every deploy.
    /// </summary>
    public bool SeedStaticData { get; init; }

    /// <summary>
    /// Seeds demo/sample data (example problems, test suites).
    /// Intended for development and staging environments only.
    /// </summary>
    public bool SeedDemoData { get; init; }

    /// <summary>
    /// Runs all pending once-only seeds tracked in the seed_history table.
    /// Each seed runs exactly once across all environments.
    /// </summary>
    public bool SeedOnce { get; init; }
}