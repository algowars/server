namespace Infrastructure.Configuration;

public sealed class Judge0Options
{
    public bool Enabled { get; init; }

    public bool RunWorker { get; init; }

    public required string BaseUrl { get; init; }

    public required string ApiKey { get; init; }

    public required string Host { get; init; }

    public bool ShouldWait { get; init; }

    public bool IsEncoded { get; init; }

    public int DefaultTimeoutInSeconds { get; init; }
}
