using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Accounts;
using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Dtos.Languages;
using ApplicationCore.Dtos.Problems.Admin;
using ApplicationCore.Interfaces.Repositories;
using ApplicationCore.Queries.Problems.GetAdminProblemsPageable;
using Ardalis.Result;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests.ApplicationCore.Queries.Problems;

[TestFixture]
public sealed class GetAdminProblemsPageableHandlerTests
{
    private Mock<IProblemRepository> _repositoryMock;
    private GetAdminProblemsPageableHandler _sut;

    [SetUp]
    public void SetUp()
    {
        _repositoryMock = new Mock<IProblemRepository>();
        _sut = new GetAdminProblemsPageableHandler(_repositoryMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnPaginatedAdminProblemDtos_WhenRepositoryReturnsData()
    {
        var problems = new[]
        {
            new ProblemModel
            {
                Id = Guid.NewGuid(),
                Title = "Problem 1",
                Status = ProblemStatus.Accepted,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = new AccountModel { Id = Guid.NewGuid(), Username = "user1" },
                ProblemSetups = new List<ProblemSetupModel>
                {
                    new ProblemSetupModel
                    {
                        Id = 1,
                        FunctionName = "",
                        InitialCode = "",
                        LanguageVersionId = 1,
                        ProblemId = Guid.NewGuid(),
                        LanguageVersion = new LanguageVersion
                        {
                            Id = 1,
                            Version = "1",
                            ProgrammingLanguage = new ProgrammingLanguage { Id = 1, Name = "C#" },
                        },
                    },
                    new ProblemSetupModel
                    {
                        LanguageVersion = new LanguageVersion
                        {
                            Id = 2,
                            Version = "1",
                            ProgrammingLanguage = new ProgrammingLanguage
                            {
                                Id = 2,
                                Name = "Python",
                            },
                        },
                        Id = 0,
                        ProblemId = Guid.NewGuid(),
                        InitialCode = "",
                        LanguageVersionId = 0,
                    },
                },
                Slug = "test",
                Question = "test",
                Tags = [new TagModel { Id = 1, Value = "test" }],
            },
        };

        var paginatedResult = new PaginatedResult<ProblemModel>
        {
            Results = problems,
            Page = 1,
            Size = 10,
            Total = problems.Length,
        };

        _repositoryMock
            .Setup(r =>
                r.GetProblemsAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(paginatedResult);

        var query = new GetAdminProblemsPageableQuery(
            new PaginationRequest { Page = 1, Size = 10 }
        );

        var result = await _sut.Handle(query, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Page, Is.EqualTo(1));
            Assert.That(result.Value.Total, Is.EqualTo(problems.Length));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnError_WhenRepositoryThrowsException()
    {
        _repositoryMock
            .Setup(r =>
                r.GetProblemsAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>())
            )
            .ThrowsAsync(new Exception("Database error"));

        var query = new GetAdminProblemsPageableQuery(
            new PaginationRequest { Page = 1, Size = 10 }
        );

        var result = await _sut.Handle(query, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsError, Is.True);
            Assert.That(result.Errors.First(), Is.EqualTo("Database error"));
        }
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyList_WhenProblemHasNoSetups()
    {
        var problems = new[]
        {
            new ProblemModel
            {
                Slug = "",
                Question = "",
                Tags = [],
                Id = Guid.NewGuid(),
                Title = "Empty Problem",
                Status = ProblemStatus.Rejected,
                CreatedOn = DateTime.UtcNow,
                ProblemSetups = new List<ProblemSetupModel>(),
            },
        };

        var paginatedResult = new PaginatedResult<ProblemModel>
        {
            Results = problems,
            Page = 1,
            Size = 10,
            Total = 1,
        };

        _repositoryMock
            .Setup(r =>
                r.GetProblemsAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(paginatedResult);

        var query = new GetAdminProblemsPageableQuery(
            new PaginationRequest { Page = 1, Size = 10 }
        );

        var result = await _sut.Handle(query, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Results.First().ProgrammingLanguages, Is.Empty);
        }
    }

    [Test]
    public async Task Handle_ShouldDeduplicateLanguages_WhenProblemHasDuplicateLanguages()
    {
        var language = new ProgrammingLanguage { Id = 1, Name = "C#" };
        var problems = new[]
        {
            new ProblemModel
            {
                Slug = "",
                Question = "",
                Tags = [],
                Id = Guid.NewGuid(),
                Title = "Duplicate Lang Problem",
                Status = ProblemStatus.Accepted,
                CreatedOn = DateTime.UtcNow,
                ProblemSetups = new List<ProblemSetupModel>
                {
                    new ProblemSetupModel
                    {
                        LanguageVersion = new LanguageVersion
                        {
                            Id = 1,
                            ProgrammingLanguage = language,
                            Version = "1",
                        },
                        Id = 0,
                        ProblemId = Guid.NewGuid(),
                        InitialCode = "",
                        LanguageVersionId = 0,
                    },
                    new ProblemSetupModel
                    {
                        LanguageVersion = new LanguageVersion
                        {
                            Id = 2,
                            ProgrammingLanguage = language,
                            Version = "2",
                        },
                        Id = 0,
                        ProblemId = Guid.NewGuid(),
                        InitialCode = "",
                        LanguageVersionId = 2,
                    },
                },
            },
        };

        var paginatedResult = new PaginatedResult<ProblemModel>
        {
            Results = problems,
            Page = 1,
            Size = 10,
            Total = 1,
        };

        _repositoryMock
            .Setup(r =>
                r.GetProblemsAsync(It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(paginatedResult);

        var query = new GetAdminProblemsPageableQuery(
            new PaginationRequest { Page = 1, Size = 10 }
        );

        var result = await _sut.Handle(query, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Results.First().ProgrammingLanguages.Count, Is.EqualTo(1));
            Assert.That(
                result.Value.Results[0].ProgrammingLanguages.First().Name,
                Is.EqualTo("C#")
            );
        }
    }
}