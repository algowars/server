using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outbox;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Interfaces.Services;
using Infrastructure.Job.Jobs;
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
                Times.Never
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

    // [Test]
    // public void ExecuteAsync_ShouldNotCallExecution_WhenNoSubmissions()
    // {
    //     _submissionRepositoryMock
    //         .Setup(repo => repo.GetSubmissionExecutionOutboxesAsync(It.IsAny<CancellationToken>()))
    //         .ReturnsAsync([
    //             new SubmissionOutboxModel
    //             {
    //                 Id = Guid.NewGuid(),
    //                 SubmissionId = Guid.NewGuid(),
    //                 Submission = new SubmissionModel { Id = Guid.NewGuid() },
    //                 Type = SubmissionOutboxType.ExecuteSubmission,
    //                 Status = SubmissionOutboxStatus.Pending,
    //             },
    //         ]);
    // }
}
