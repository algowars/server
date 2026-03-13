namespace ApplicationCore.Domain.CodeExecution;

public sealed class ComparisonContext
{
    public required Guid SubmissionId { get; init; }

    public required IEnumerable<ComparisonBuildResult> BuiltResults { get; init; } = [];
}
