using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Domain.Problems.TestSuites;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;
using Infrastructure.Job.Jobs;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;

namespace UnitTests.Infrastructure.Job;

[TestFixture]
public sealed class SubmissionExecutorHandlerTests
{
    private SubmissionExecutorHandler _sut;
    private Mock<IProblemAppService> _problemAppServiceMock;
    private Mock<ICodeBuilderService> _codeBuilderServiceMock;
    private Mock<ISubmissionRepository> _submissionRepositoryMock;
    private Mock<ICodeExecutionService> _codeExecutionServiceMock;

    [SetUp]
    public void SetUp()
    {
        _problemAppServiceMock = new();
        _codeBuilderServiceMock = new();
        _submissionRepositoryMock = new();
        _codeExecutionServiceMock = new();

        _sut = new SubmissionExecutorHandler(
            _problemAppServiceMock.Object,
            _codeBuilderServiceMock.Object,
            _submissionRepositoryMock.Object,
            _codeExecutionServiceMock.Object
        );
    }

    [Test]
    public async Task ExecuteAsync_ShouldNotCallExecution_WhenNoSubmissions()
    {
        _submissionRepositoryMock
            .Setup(repo => repo.GetSubmissionExecutionOutboxesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _problemAppServiceMock
            .Setup(service =>
                service.GetProblemSetupsForExecutionAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Success<IEnumerable<ProblemSetupModel>>([]));

        await _sut.ExecuteAsync(CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            _submissionRepositoryMock.Verify(
                repo => repo.GetSubmissionExecutionOutboxesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
            _problemAppServiceMock.Verify(
                service =>
                    service.GetProblemSetupsForExecutionAsync(
                        It.IsAny<IEnumerable<int>>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
            _codeBuilderServiceMock.Verify(
                service => service.Build(It.IsAny<IEnumerable<CodeBuilderContext>>()),
                Times.Never
            );
            _codeExecutionServiceMock.Verify(
                service =>
                    service.ExecuteAsync(
                        It.IsAny<IEnumerable<CodeExecutionContext>>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
            _submissionRepositoryMock.Verify(
                repo =>
                    repo.ProcessSubmissionExecution(
                        It.IsAny<IEnumerable<SubmissionModel>>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }
    }

    [Test]
    public async Task ExecuteAsync_BuildsAndExecuteSubmissions_WhenThereIsOutboxesToRun()
    {
        var cancellationToken = CancellationToken.None;

        var submissionId = Guid.NewGuid();
        var outboxId = Guid.NewGuid();

        var setup = new ProblemSetupModel
        {
            Id = 42,
            ProblemId = Guid.NewGuid(),
            InitialCode = "",
            LanguageVersionId = 1,
            FunctionName = "Solve",
            HarnessTemplate = new HarnessTemplate { Id = 1, Template = "template" },
            TestSuites =
            [
                new TestSuiteModel
                {
                    TestCases =
                    [
                        new TestCaseModel
                        {
                            Id = 1,
                            Input = "test",
                            ExpectedOutput = "",
                            TestCaseType = TestCaseType.Sample,
                        },
                    ],
                },
            ],
        };

        var outbox = new SubmissionOutboxModel
        {
            Id = outboxId,
            SubmissionId = submissionId,
            Submission = new SubmissionModel { Id = submissionId, ProblemSetupId = setup.Id },
            Type = SubmissionOutboxType.ExecuteSubmission,
            Status = SubmissionOutboxStatus.Pending,
        };

        _submissionRepositoryMock
            .Setup(r => r.GetSubmissionExecutionOutboxesAsync(cancellationToken))
            .ReturnsAsync([outbox]);

        _problemAppServiceMock
            .Setup(s =>
                s.GetProblemSetupsForExecutionAsync(It.IsAny<IEnumerable<int>>(), cancellationToken)
            )
            .ReturnsAsync(Result.Success<IEnumerable<ProblemSetupModel>>([setup]));

        var buildResult = new CodeBuildResult { FinalCode = "test code", FunctionName = "test" };

        _codeBuilderServiceMock
            .Setup(s => s.Build(It.IsAny<IEnumerable<CodeBuilderContext>>()))
            .Returns(Result.Success<IEnumerable<CodeBuildResult>>([buildResult]));

        var executedSubmission = new SubmissionModel { Id = submissionId };

        _codeExecutionServiceMock
            .Setup(s =>
                s.ExecuteAsync(It.IsAny<IEnumerable<CodeExecutionContext>>(), cancellationToken)
            )
            .ReturnsAsync(Result.Success<IEnumerable<SubmissionModel>>([executedSubmission]));

        await _sut.ExecuteAsync(cancellationToken);

        using (Assert.EnterMultipleScope())
        {
            _submissionRepositoryMock.Verify(
                r => r.GetSubmissionExecutionOutboxesAsync(cancellationToken),
                Times.Once
            );

            _problemAppServiceMock.Verify(
                s =>
                    s.GetProblemSetupsForExecutionAsync(
                        It.Is<IEnumerable<int>>(ids => ids.Contains(setup.Id)),
                        cancellationToken
                    ),
                Times.Once
            );

            _codeBuilderServiceMock.Verify(
                s => s.Build(It.IsAny<IEnumerable<CodeBuilderContext>>()),
                Times.Once
            );

            _codeExecutionServiceMock.Verify(
                s =>
                    s.ExecuteAsync(
                        It.IsAny<IEnumerable<CodeExecutionContext>>(),
                        cancellationToken
                    ),
                Times.Once
            );

            _submissionRepositoryMock.Verify(
                r =>
                    r.ProcessSubmissionExecution(
                        It.Is<IEnumerable<SubmissionModel>>(subs =>
                            subs.Any(s => s.Id == submissionId)
                        ),
                        cancellationToken
                    ),
                Times.Once
            );
        }
    }
}
