using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Clients;
using ApplicationCore.Services;
using Ardalis.Result;
using Moq;

namespace UnitTests.ApplicationCore.Services;

[TestFixture]
public sealed class CodeExecutionServiceTests
{
    private Mock<IJudge0Client> _mockJudge0Client = null!;
    private CodeExecutionService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockJudge0Client = new Mock<IJudge0Client>();
        _sut = new CodeExecutionService(_mockJudge0Client.Object);
    }

    private static ProblemSetupModel CreateSetup() =>
        new()
        {
            Id = 1,
            ProblemId = Guid.NewGuid(),
            InitialCode = "function f() {}",
            LanguageVersionId = 1,
        };

    private static CodeExecutionContext CreateContext(Guid? submissionId = null) =>
        new()
        {
            SubmissionId = submissionId ?? Guid.NewGuid(),
            Setup = CreateSetup(),
            Code = "function f() {}",
            CreatedById = Guid.NewGuid(),
            BuiltResults =
            [
                new CodeBuildResult
                {
                    FinalCode = "console.log('hello');",
                    FunctionName = "f",
                    Inputs = "1",
                    ExpectedOutput = "1",
                },
            ],
        };

    [Test]
    public async Task ExecuteAsync_returns_empty_when_no_contexts()
    {
        var result = await _sut.ExecuteAsync([], CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        }
    }

    [Test]
    public async Task ExecuteAsync_returns_error_when_judge0_fails()
    {
        _mockJudge0Client
            .Setup(j =>
                j.SubmitAsync(
                    It.IsAny<IEnumerable<Judge0SubmissionRequest>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Error("Service unavailable"));

        var context = CreateContext();
        var result = await _sut.ExecuteAsync([context], CancellationToken.None);

        Assert.That(result.IsError(), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_returns_submission_models_with_judge0_tokens()
    {
        var submissionId = Guid.NewGuid();
        var token = Guid.NewGuid();

        _mockJudge0Client
            .Setup(j =>
                j.SubmitAsync(
                    It.IsAny<IEnumerable<Judge0SubmissionRequest>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Result.Success(
                    new List<Judge0SubmissionResponse>
                    {
                        new()
                        {
                            Token = token,
                            Status = new Judge0StatusModel { Id = 1 },
                        },
                    }
                )
            );

        var context = CreateContext(submissionId);
        var result = await _sut.ExecuteAsync([context], CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Exactly(1).Items);
            Assert.That(result.Value.First().Id, Is.EqualTo(submissionId));
            Assert.That(result.Value.First().Results.First().Id, Is.EqualTo(token));
            Assert.That(
                result.Value.First().Results.First().Status,
                Is.EqualTo(SubmissionStatus.InQueue)
            );
        }
    }

    [Test]
    public async Task ExecuteAsync_returns_error_when_judge0_response_count_mismatches()
    {
        _mockJudge0Client
            .Setup(j =>
                j.SubmitAsync(
                    It.IsAny<IEnumerable<Judge0SubmissionRequest>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Success(new List<Judge0SubmissionResponse>()));

        var context = CreateContext();
        var result = await _sut.ExecuteAsync([context], CancellationToken.None);

        Assert.That(result.IsError(), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_returns_success_when_context_has_no_built_results()
    {
        var context = new CodeExecutionContext
        {
            SubmissionId = Guid.NewGuid(),
            Setup = CreateSetup(),
            Code = "function f() {}",
            CreatedById = Guid.NewGuid(),
            BuiltResults = [],
        };

        var result = await _sut.ExecuteAsync([context], CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Has.Exactly(1).Items);
            Assert.That(result.Value.First().Results, Is.Empty);
        }
    }

    [Test]
    public async Task GetSubmissionResultsAsync_returns_empty_when_no_submissions()
    {
        var result = await _sut.GetSubmissionResultsAsync([], CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);
        }
    }

    [Test]
    public async Task GetSubmissionResultsAsync_returns_error_when_judge0_fails()
    {
        _mockJudge0Client
            .Setup(j =>
                j.GetAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(Result.Error("Service unavailable"));

        var token = Guid.NewGuid();
        var submissions = new List<SubmissionModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CreatedById = Guid.NewGuid(),
                Results = [new SubmissionResult { Id = token, Status = SubmissionStatus.InQueue }],
            },
        };

        var result = await _sut.GetSubmissionResultsAsync(submissions, CancellationToken.None);

        Assert.That(result.IsError(), Is.True);
    }

    [Test]
    public async Task GetSubmissionResultsAsync_updates_submission_results_from_judge0_response()
    {
        var token = Guid.NewGuid();
        var submissions = new List<SubmissionModel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CreatedById = Guid.NewGuid(),
                Results = [new SubmissionResult { Id = token, Status = SubmissionStatus.InQueue }],
            },
        };

        _mockJudge0Client
            .Setup(j =>
                j.GetAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                Result.Success(
                    new List<Judge0SubmissionResponse>
                    {
                        new()
                        {
                            Token = token,
                            Stdout = "expected",
                            Time = "0.050",
                            Memory = 2048,
                            Status = new Judge0StatusModel { Id = 3 },
                        },
                    }
                )
            );

        var result = await _sut.GetSubmissionResultsAsync(submissions, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            var updatedResult = result.Value.First().Results.First();
            Assert.That(updatedResult.Status, Is.EqualTo(SubmissionStatus.Accepted));
            Assert.That(updatedResult.Stdout, Is.EqualTo("expected"));
            Assert.That(updatedResult.MemoryKb, Is.EqualTo(2048));
            Assert.That(updatedResult.RuntimeMs, Is.EqualTo(50));
        }
    }
}
