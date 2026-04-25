using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Services;

namespace ApplicationCore.Services;

public sealed class ExecutionComparisonService : IExecutionComparisonService
{
    public SubmissionStatus Compare(string? actualOutput, string expectedOutput)
    {
        if (actualOutput is null)
        {
            return SubmissionStatus.WrongAnswer;
        }

        string actual = Normalize(actualOutput);
        string expected = Normalize(expectedOutput);

        return actual == expected ? SubmissionStatus.Accepted : SubmissionStatus.WrongAnswer;
    }

    private static string Normalize(string value) =>
        value.Trim().ReplaceLineEndings("\n");
}
