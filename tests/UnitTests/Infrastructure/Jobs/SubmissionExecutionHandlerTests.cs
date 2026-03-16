using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Domain.Problems.TestSuites;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using Ardalis.Result;
using Infrastructure.Jobs.JobHandlers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Quartz;

namespace UnitTests.Infrastructure.Jobs;

[TestFixture]
public sealed class SubmissionExecutionHandlerTests
{
    private Mock<IServiceScopeFactory> _mockScopeFactory = null!;
    private Mock<IServiceScope> _mockScope = null!;
    private Mock<IServiceProvider> _mockServiceProvider = null!;
    private Mock<ISubmissionAppService> _mockSubmissionAppService = null!;
    private Mock<IProblemAppService> _mockProblemAppService = null!;
    private Mock<ICodeBuilderService> _mockCodeBuilderService = null!;
    private Mock<ICodeExecutionService> _mockCodeExecutionService = null!;
    private Mock<IJobExecutionContext> _mockJobContext = null!;

    private SubmissionExecutionHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockSubmissionAppService = new Mock<ISubmissionAppService>();
        _mockProblemAppService = new Mock<IProblemAppService>();
        _mockCodeBuilderService = new Mock<ICodeBuilderService>();
        _mockCodeExecutionService = new Mock<ICodeExecutionService>();

        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ISubmissionAppService)))
            .Returns(_mockSubmissionAppService.Object);
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IProblemAppService)))
            .Returns(_mockProblemAppService.Object);
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ICodeBuilderService)))
            .Returns(_mockCodeBuilderService.Object);
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ICodeExecutionService)))
            .Returns(_mockCodeExecutionService.Object);

        _mockScope = new Mock<IServiceScope>();
        _mockScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);

        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockScope.Object);

        _mockJobContext = new Mock<IJobExecutionContext>();
        _mockJobContext.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        _sut = new SubmissionExecutionHandler(_mockScopeFactory.Object);
    }

    private static SubmissionOutboxModel CreateOutbox(
        SubmissionOutboxType type = SubmissionOutboxType.Initialized
    )
    {
        var submissionId = Guid.NewGuid();
        return new SubmissionOutboxModel
        {
            Id = Guid.NewGuid(),
            SubmissionId = submissionId,
            Submission = new SubmissionModel
            {
                Id = submissionId,
                Code = "function f() {}",
                ProblemSetupId = 1,
                CreatedById = Guid.NewGuid(),
            },
            Type = type,
            Status = SubmissionOutboxStatus.Pending,
        };
    }

    private static ProblemSetupModel CreateSetup(int id = 1) =>
        new()
        {
            Id = id,
            ProblemId = Guid.NewGuid(),
            InitialCode = "function f() {}",
            FunctionName = "f",
            LanguageVersionId = 1,
            HarnessTemplate = new HarnessTemplate { Id = 1, Template = "{{USER_CODE}}" },
            TestSuites =
            [
                new TestSuiteModel
                {
                    TestCases =
                    [
                        new TestCaseModel { Id = 1, Input = "1", ExpectedOutput = "1" },
                    ],
                },
            ],
        };

    [Test]
    public async Task Execute_returns_early_when_outbox_fetch_fails()
    {
        _mockSubmissionAppService
            .Setup(s => s.GetSubmissionOutboxesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Error("db error"));

        await _sut.Execute(_mockJobContext.Object);

        _mockProblemAppService.Verify(
            s =>
                s.GetProblemSetupsForExecutionAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task Execute_returns_early_when_no_outboxes()
    {
        _mockSubmissionAppService
            .Setup(s => s.GetSubmissionOutboxesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<SubmissionOutboxModel>>([]));

        await _sut.Execute(_mockJobContext.Object);

        _mockProblemAppService.Verify(
            s =>
                s.GetProblemSetupsForExecutionAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task Execute_returns_early_when_no_initialized_outboxes()
    {
        var outbox = CreateOutbox(SubmissionOutboxType.Evaluate);

        _mockSubmissionAppService
            .Setup(s => s.GetSubmissionOutboxesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<SubmissionOutboxModel>>([outbox]));

        _mockProblemAppService
            .Setup(s =>
                s.GetProblemSetupsForExecutionAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Success<IEnumerable<ProblemSetupModel>>([]));

        await _sut.Execute(_mockJobContext.Object);

        _mockSubmissionAppService.Verify(
            s =>
                s.IncrementOutboxesCountAsync(
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Test]
    public async Task Execute_runs_full_pipeline_for_initialized_outboxes()
    {
        var outbox = CreateOutbox();
        var setup = CreateSetup(outbox.Submission.ProblemSetupId);

        _mockSubmissionAppService
            .Setup(s => s.GetSubmissionOutboxesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<IEnumerable<SubmissionOutboxModel>>([outbox]));

        _mockProblemAppService
            .Setup(s =>
                s.GetProblemSetupsForExecutionAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Success<IEnumerable<ProblemSetupModel>>([setup]));

        _mockCodeBuilderService
            .Setup(s => s.Build(It.IsAny<IEnumerable<CodeBuilderContext>>()))
            .Returns(
                Result.Success<IEnumerable<CodeBuildResult>>(
                    [
                        new CodeBuildResult
                        {
                            FinalCode = "console.log('hello');",
                            FunctionName = "f",
                            Inputs = "1",
                            ExpectedOutput = "1",
                        },
                    ]
                )
            );

        var submissionResult = new List<SubmissionModel>
        {
            new() { Id = outbox.SubmissionId, CreatedById = outbox.Submission.CreatedById },
        };

        _mockCodeExecutionService
            .Setup(s =>
                s.ExecuteAsync(
                    It.IsAny<IEnumerable<CodeExecutionContext>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Success<IEnumerable<SubmissionModel>>(submissionResult));

        _mockSubmissionAppService
            .Setup(s =>
                s.IncrementOutboxesCountAsync(
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Success());

        _mockSubmissionAppService
            .Setup(s =>
                s.ProcessSubmissionExecutionAsync(
                    It.IsAny<IEnumerable<SubmissionModel>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Success());

        await _sut.Execute(_mockJobContext.Object);

        _mockSubmissionAppService.Verify(
            s =>
                s.IncrementOutboxesCountAsync(
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _mockCodeExecutionService.Verify(
            s =>
                s.ExecuteAsync(
                    It.IsAny<IEnumerable<CodeExecutionContext>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _mockSubmissionAppService.Verify(
            s =>
                s.ProcessSubmissionExecutionAsync(
                    It.IsAny<IEnumerable<SubmissionModel>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
