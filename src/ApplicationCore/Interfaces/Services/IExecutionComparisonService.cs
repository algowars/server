using ApplicationCore.Domain.Submissions;

namespace ApplicationCore.Interfaces.Services;

public interface IExecutionComparisonService
{
    SubmissionStatus Compare(string? actualOutput, string expectedOutput);
}