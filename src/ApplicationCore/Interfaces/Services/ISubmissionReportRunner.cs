using ApplicationCore.Domain.Problems.TestSuites;
using ApplicationCore.Domain.Submissions;

namespace ApplicationCore.Interfaces.Services;

public interface ISubmissionReportRunner
{
    IEnumerable<SubmissionResult> EvaluateResults(
        IEnumerable<SubmissionResult> results,
        IEnumerable<TestCaseModel> testCases
    );
}
