using ApplicationCore.Domain.Problems.TestSuites;

namespace ApplicationCore.Domain.CodeExecution;

public sealed class CodeBuilderContext
{
    public required string Code { get; init; }

    public required string Template { get; init; }

    public required string FunctionName { get; init; }

    public int? LanguageVersionId { get; init; }

    public int? Judge0LanguageId { get; init; }

    public required IEnumerable<TestCaseInputParamModel> Inputs { get; init; }

    public required TestCaseExpectedOutputModel ExpectedOutput { get; init; }

    public string? InputTypeName { get; init; }
}