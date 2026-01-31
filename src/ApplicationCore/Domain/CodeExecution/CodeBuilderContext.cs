namespace ApplicationCore.Domain.CodeExecution;

public sealed class CodeBuilderContext
{
    public required string Code { get; init; }

    public required string Template { get; init; }

    public required string FunctionName { get; init; }

    public int? LanguageVersionId { get; init; }

    public required string Inputs { get; init; }

    public required string ExpectedOutput { get; init; }

    public string? InputTypeName { get; init; }
}
