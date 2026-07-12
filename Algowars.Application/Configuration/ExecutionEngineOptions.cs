using Algowars.Application.Settings;

namespace Algowars.Application.Configuration;

public sealed class ExecutionEngineOptions : IOption
{
    public static string SectionName => "ExecutionEngines";

    public Judge0Options Judge0 { get; init; } = new();
}

public sealed class Judge0Options
{
    public bool Enabled { get; init; } = true;
    public bool RunWorker { get; init; } = true;
    public string BaseUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
    public bool ShouldWait { get; init; } = false;
    public bool IsEncoded { get; init; } = true;
    public int DefaultTimeoutInSeconds { get; init; } = 10;
}
