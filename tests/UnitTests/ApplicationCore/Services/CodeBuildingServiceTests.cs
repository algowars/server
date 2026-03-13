using ApplicationCore.Domain.CodeExecution;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Domain.Problems.TestSuites;
using ApplicationCore.Domain.Submissions;
using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Services;
using Ardalis.Result;
using Moq;

namespace UnitTests.ApplicationCore.Services;

[TestFixture]
public sealed class CodeBuildingServiceTests
{
    private Mock<ICodeBuilderService> _mockCodeBuilderService;
    private CodeBuildingService _sut;

    [SetUp]
    public void SetUp()
    {
        _mockCodeBuilderService = new Mock<ICodeBuilderService>();
        _sut = new CodeBuildingService(_mockCodeBuilderService.Object);
    }

    [Test]
    public void BuildExecutionContexts_HappyPath_ReturnsTwoContextsWithCorrectData()
    {
        var submissionId = Guid.NewGuid();
        var createdById = Guid.NewGuid();

        var outbox = new SubmissionOutboxModel
        {
            Id = Guid.NewGuid(),
            SubmissionId = submissionId,
            Type = SubmissionOutboxType.Initialized,
            Status = SubmissionOutboxStatus.Pending,
            Submission = new SubmissionModel
            {
                Id = submissionId,
                Code = "function foo() {}",
                ProblemSetupId = 1,
                CreatedById = createdById,
            },
        };

        var setup = new ProblemSetupModel
        {
            Id = 1,
            ProblemId = Guid.NewGuid(),
            InitialCode = "// initial",
            FunctionName = "foo",
            LanguageVersionId = 10,
            HarnessTemplate = new HarnessTemplate { Id = 1, Template = "{{USER_CODE}}" },
            TestSuites =
            [
                new TestSuiteModel
                {
                    Id = 1,
                    TestSuiteType = TestSuiteType.Public,
                    TestCases =
                    [
                        new TestCaseModel
                        {
                            Id = 1,
                            Input = "[1,2]",
                            ExpectedOutput = "3",
                        },
                        new TestCaseModel
                        {
                            Id = 2,
                            Input = "[3,4]",
                            ExpectedOutput = "7",
                        },
                    ],
                },
            ],
        };

        var builtResults = new List<CodeBuildResult>
        {
            new CodeBuildResult
            {
                FinalCode = "function foo() {} // built1",
                FunctionName = "foo",
                Inputs = "[1,2]",
                ExpectedOutput = "3",
            },
            new CodeBuildResult
            {
                FinalCode = "function foo() {} // built2",
                FunctionName = "foo",
                Inputs = "[3,4]",
                ExpectedOutput = "7",
            },
        };

        _mockCodeBuilderService
            .Setup(s => s.Build(It.IsAny<IEnumerable<CodeBuilderContext>>()))
            .Returns(Result<IEnumerable<CodeBuildResult>>.Success(builtResults));

        var outboxes = new List<SubmissionOutboxModel> { outbox };
        var setupsMap = new Dictionary<int, ProblemSetupModel> { { setup.Id, setup } };

        var result = _sut.BuildExecutionContexts(outboxes, setupsMap);

        Assert.That(result.IsSuccess, Is.True);
        var contexts = result.Value.ToList();
        Assert.That(contexts, Has.Count.EqualTo(1));

        var ctx = contexts[0];
        Assert.That(ctx.SubmissionId, Is.EqualTo(submissionId));
        Assert.That(ctx.Code, Is.EqualTo("function foo() {}"));
        Assert.That(ctx.CreatedById, Is.EqualTo(createdById));
        Assert.That(ctx.BuiltResults.Count(), Is.EqualTo(2));
    }

    [Test]
    public void BuildExecutionContexts_BuildFailure_PropagatesError()
    {
        var submissionId = Guid.NewGuid();

        var outbox = new SubmissionOutboxModel
        {
            Id = Guid.NewGuid(),
            SubmissionId = submissionId,
            Type = SubmissionOutboxType.Initialized,
            Status = SubmissionOutboxStatus.Pending,
            Submission = new SubmissionModel
            {
                Id = submissionId,
                Code = "",
                ProblemSetupId = 1,
                CreatedById = Guid.NewGuid(),
            },
        };

        var setup = new ProblemSetupModel
        {
            Id = 1,
            ProblemId = Guid.NewGuid(),
            InitialCode = "// initial",
            FunctionName = "foo",
            LanguageVersionId = 10,
            TestSuites =
            [
                new TestSuiteModel
                {
                    Id = 1,
                    TestSuiteType = TestSuiteType.Public,
                    TestCases =
                    [
                        new TestCaseModel
                        {
                            Id = 1,
                            Input = "[1]",
                            ExpectedOutput = "1",
                        },
                    ],
                },
            ],
        };

        _mockCodeBuilderService
            .Setup(s => s.Build(It.IsAny<IEnumerable<CodeBuilderContext>>()))
            .Returns(Result<IEnumerable<CodeBuildResult>>.Error("Initial code is required."));

        var result = _sut.BuildExecutionContexts(
            new List<SubmissionOutboxModel> { outbox },
            new Dictionary<int, ProblemSetupModel> { { setup.Id, setup } }
        );

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Does.Contain("Initial code is required."));
    }

    [Test]
    public void BuildExecutionContexts_EmptyTestCases_ReturnsOneContextWithEmptyBuiltResults()
    {
        var submissionId = Guid.NewGuid();

        var outbox = new SubmissionOutboxModel
        {
            Id = Guid.NewGuid(),
            SubmissionId = submissionId,
            Type = SubmissionOutboxType.Initialized,
            Status = SubmissionOutboxStatus.Pending,
            Submission = new SubmissionModel
            {
                Id = submissionId,
                Code = "function foo() {}",
                ProblemSetupId = 2,
                CreatedById = Guid.NewGuid(),
            },
        };

        var setup = new ProblemSetupModel
        {
            Id = 2,
            ProblemId = Guid.NewGuid(),
            InitialCode = "// initial",
            FunctionName = "foo",
            LanguageVersionId = 10,
            TestSuites = [],
        };

        var emptyResults = Enumerable.Empty<CodeBuildResult>();

        _mockCodeBuilderService
            .Setup(s => s.Build(It.IsAny<IEnumerable<CodeBuilderContext>>()))
            .Returns(Result<IEnumerable<CodeBuildResult>>.Success(emptyResults));

        var result = _sut.BuildExecutionContexts(
            new List<SubmissionOutboxModel> { outbox },
            new Dictionary<int, ProblemSetupModel> { { setup.Id, setup } }
        );

        Assert.That(result.IsSuccess, Is.True);
        var contexts = result.Value.ToList();
        Assert.That(contexts, Has.Count.EqualTo(1));
        Assert.That(contexts[0].BuiltResults, Is.Empty);
    }

    [Test]
    public void BuildExecutionContexts_MissingSetup_ReturnsError()
    {
        var submissionId = Guid.NewGuid();

        var outbox = new SubmissionOutboxModel
        {
            Id = Guid.NewGuid(),
            SubmissionId = submissionId,
            Type = SubmissionOutboxType.Initialized,
            Status = SubmissionOutboxStatus.Pending,
            Submission = new SubmissionModel
            {
                Id = submissionId,
                Code = "function foo() {}",
                ProblemSetupId = 99,
                CreatedById = Guid.NewGuid(),
            },
        };

        var emptySetupsMap = new Dictionary<int, ProblemSetupModel>();

        var result = _sut.BuildExecutionContexts(
            new List<SubmissionOutboxModel> { outbox },
            emptySetupsMap
        );

        Assert.That(result.IsSuccess, Is.False);
    }
}
