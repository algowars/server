namespace ApplicationCore.Domain.Problems.TestSuites;

public sealed class TestCaseInputParamModel
{
    public int Id { get; set; }

    public required string Value { get; set; }

    public int TestCaseInputValueTypeId { get; set; }

    public required TestCaseInputValueTypeModel InputType { get; set; }
}