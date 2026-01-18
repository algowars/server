namespace ApplicationCore.Domain.CodeExecution;

public sealed class CodeBuildResult
{
    public required string FinalCode { get; init; }

    public required string FunctionName { get; init; }

    public string? Inputs { get; init; }

    public string? ExpectedOutputs { get; init; }

    public string? InputTypeName { get; init; }
}
