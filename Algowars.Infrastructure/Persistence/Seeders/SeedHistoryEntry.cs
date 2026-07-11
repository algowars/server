namespace Algowars.Infrastructure.Persistence.Seeders;

/// <summary>
/// Tracks run-once seeds by name so they are never executed twice.
/// </summary>
internal sealed class SeedHistoryEntry
{
    public SeedHistoryEntry(string name)
    {
        Name = !string.IsNullOrWhiteSpace(name)
            ? name
            : throw new ArgumentException("Seed name must not be empty.", nameof(name));

        ExecutedAt = DateTime.UtcNow;
    }

    private SeedHistoryEntry() { }

    public string Name { get; private set; } = null!;
    public DateTime ExecutedAt { get; private set; }
}
