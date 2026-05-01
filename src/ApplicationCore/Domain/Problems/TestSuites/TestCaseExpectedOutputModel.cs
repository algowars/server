

namespace ApplicationCore.Domain.Problems.TestSuites;

public sealed class TestCaseExpectedOutputModel
{
    public int Id { get; set; }

    public int TestCaseId { get; set; }

    public string Value { get; set; }

    public int OutputValueTypeId { get; set; }

    public TestCaseOutputTypeModel OutputType { get; set; }
}
