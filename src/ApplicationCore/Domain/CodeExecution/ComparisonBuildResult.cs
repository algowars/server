namespace ApplicationCore.Domain.CodeExecution;

public sealed class ComparisonBuildResult
{
    public Guid ExecutionId { get; set; }

    public Guid ResultId { get; set; }

    public required string ActualOutput { get; init; }

    public required string ExpectedOutput { get; init; }
}
