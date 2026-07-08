namespace Algowars.Application.Problems.Dtos;

public sealed record ProblemSetupTestCaseDto(string Inputs, string ExpectedOutput);

public sealed record ProblemSetupDto(Guid Id, string InitialCode, IEnumerable<ProblemSetupTestCaseDto> TestCases);