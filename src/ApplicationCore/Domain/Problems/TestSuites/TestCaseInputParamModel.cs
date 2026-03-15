namespace ApplicationCore.Domain.Problems.TestSuites;

public sealed class TestCaseInputParamModel
{
    public int Id { get; set; }

    public string Value { get; set; }

    public int TestCaseInputValueTypeId { get; set; }

    public TestCaseInputValueTypeModel InputType { get; set; }
}
