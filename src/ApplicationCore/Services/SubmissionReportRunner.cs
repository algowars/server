using ApplicationCore.Domain.Problems.TestSuites;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Services;

namespace ApplicationCore.Services;

public sealed class SubmissionReportRunner : ISubmissionReportRunner
{
    public IEnumerable<SubmissionResult> EvaluateResults(
        IEnumerable<SubmissionResult> results,
        IEnumerable<TestCaseModel> testCases
    )
    {
        var resultList = results.ToList();
        var testCaseList = testCases.ToList();

        for (var i = 0; i < resultList.Count; i++)
        {
            var result = resultList[i];

            if (result.Status is not SubmissionStatus.Processing)
            {
                continue;
            }

            if (i >= testCaseList.Count)
            {
                result.Status = SubmissionStatus.WrongAnswer;
                continue;
            }

            var testCase = testCaseList[i];
            var parsedOutput = ParseLastLine(result.ProgramOutput);
            result.Stdout = parsedOutput;

            result.Status = string.Equals(parsedOutput, testCase.ExpectedOutput, StringComparison.Ordinal)
                ? SubmissionStatus.Accepted
                : SubmissionStatus.WrongAnswer;
        }

        return resultList;
    }

    private static string ParseLastLine(string? programOutput)
    {
        if (string.IsNullOrEmpty(programOutput))
        {
            return string.Empty;
        }

        var lines = programOutput.Split('\n');

        for (var i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i].TrimEnd('\r');

            if (!string.IsNullOrWhiteSpace(line))
            {
                return line;
            }
        }

        return string.Empty;
    }
}
