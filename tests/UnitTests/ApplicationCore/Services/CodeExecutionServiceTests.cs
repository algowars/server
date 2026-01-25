using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.CodeExecution.Judge0;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Interfaces.Clients;
using ApplicationCore.Services;
using Ardalis.Result;
using Moq;
using NUnit.Framework;

namespace UnitTests.ApplicationCore.Services;

[TestFixture]
public sealed class CodeExecutionServiceTests
{
    private CodeExecutionService _sut;
    private Mock<IJudge0Client> _mockJudge0Client;

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
            InitialCode = "",
            LanguageVersionId = 1,
            Version = 1,
        };

    [Test]
    public async Task ExecuteAsync_ShouldReturnEmpty_WhenContextsEmpty()
    {
        IEnumerable<CodeExecutionContext> contexts = [];

        var result = await _sut.ExecuteAsync(contexts, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.Empty);

            _mockJudge0Client.Verify(
                c =>
                    c.SubmitAsync(
                        It.IsAny<IEnumerable<Judge0SubmissionRequest>>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnSubmission_WhenNoBuiltResults()
    {
        var contexts = new[]
        {
            new CodeExecutionContext
            {
                SubmissionId = Guid.NewGuid(),
                CreatedById = Guid.NewGuid(),
                Code = "code",
                Setup = CreateSetup(),
                BuiltResults = [],
            },
        };

        var result = await _sut.ExecuteAsync(contexts, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.ToList(), Has.Count.EqualTo(1));
            Assert.That(result.Value.Single().Results, Is.Empty);

            _mockJudge0Client.Verify(
                c =>
                    c.SubmitAsync(
                        It.IsAny<IEnumerable<Judge0SubmissionRequest>>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }
    }

    [Test]
    public async Task ExecuteAsync_ShouldSubmitAndMapJudge0Results()
    {
        var token = Guid.NewGuid();

        var contexts = new[]
        {
            new CodeExecutionContext
            {
                SubmissionId = Guid.NewGuid(),
                CreatedById = Guid.NewGuid(),
                Code = "print(x)",
                Setup = CreateSetup(),
                BuiltResults =
                [
                    new CodeBuildResult
                    {
                        FinalCode = "print(x)",
                        FunctionName = "main",
                        Inputs = "1",
                        ExpectedOutput = "1",
                    },
                ],
            },
        };

        _mockJudge0Client
            .Setup(c =>
                c.SubmitAsync(
                    It.IsAny<IEnumerable<Judge0SubmissionRequest>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Result.Success(
                    new List<Judge0SubmissionResponse>
                    {
                        new Judge0SubmissionResponse
                        {
                            Token = token,
                            Status = new Judge0StatusModel { Id = 1 },
                        },
                    }
                )
            );

        var result = await _sut.ExecuteAsync(contexts, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);

            var submission = result.Value.Single();
            var submissionResult = submission.Results.Single();

            Assert.That(submissionResult.Id, Is.EqualTo(token));
            Assert.That(submissionResult.Status, Is.EqualTo(SubmissionStatus.InQueue));
        }
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnError_WhenJudge0SubmitFails()
    {
        var contexts = new[]
        {
            new CodeExecutionContext
            {
                Code = "code",
                CreatedById = Guid.NewGuid(),
                Setup = CreateSetup(),
                BuiltResults =
                [
                    new CodeBuildResult
                    {
                        FinalCode = "code",
                        FunctionName = "main",
                        Inputs = "",
                        ExpectedOutput = "",
                    },
                ],
            },
        };

        _mockJudge0Client
            .Setup(c =>
                c.SubmitAsync(
                    It.IsAny<IEnumerable<Judge0SubmissionRequest>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Error("Judge0 unavailable"));

        var result = await _sut.ExecuteAsync(contexts, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors.Single(), Is.EqualTo("Failed to submit code for execution."));
        }
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnError_WhenResponseCountMismatch()
    {
        var contexts = new[]
        {
            new CodeExecutionContext
            {
                Code = "code",
                CreatedById = Guid.NewGuid(),
                Setup = CreateSetup(),
                BuiltResults =
                [
                    new CodeBuildResult
                    {
                        FinalCode = "a",
                        FunctionName = "f",
                        Inputs = "1",
                        ExpectedOutput = "1",
                    },
                    new CodeBuildResult
                    {
                        FinalCode = "b",
                        FunctionName = "f",
                        Inputs = "2",
                        ExpectedOutput = "2",
                    },
                ],
            },
        };

        _mockJudge0Client
            .Setup(c =>
                c.SubmitAsync(
                    It.IsAny<IEnumerable<Judge0SubmissionRequest>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                Result.Success(
                    new List<Judge0SubmissionResponse>
                    {
                        new Judge0SubmissionResponse
                        {
                            Token = Guid.NewGuid(),
                            Status = new Judge0StatusModel { Id = 1 },
                        },
                    }
                )
            );

        var result = await _sut.ExecuteAsync(contexts, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(
                result.Errors.Single(),
                Is.EqualTo("Mismatch between Judge0 responses and submitted jobs.")
            );
        }
    }
}
