using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Services;

namespace ApplicationCore.Services;

public sealed class ExecutionComparisonService : IExecutionComparisonService
{
    public SubmissionStatus Compare(string? programOutput, string expectedOutput)
    {
        if (programOutput is null)
        {
            return SubmissionStatus.WrongAnswer;
        }

        string[] lines = programOutput.ReplaceLineEndings("\n").TrimEnd('\n').Split('\n');
        string lastLine = lines[^1];

        string actual = Normalize(lastLine);
        string expected = Normalize(expectedOutput);

        return actual == expected ? SubmissionStatus.Accepted : SubmissionStatus.WrongAnswer;
    }

    private static string Normalize(string value) => value.Trim().ReplaceLineEndings("\n");
}
