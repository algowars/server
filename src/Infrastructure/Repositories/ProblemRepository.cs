using System;
using System.Collections.Generic;
using System.Text;
using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Accounts;
using ApplicationCore.Domain.Problems;
using ApplicationCore.Domain.Problems.Languages;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Domain.Problems.TestSuites;
using ApplicationCore.Interfaces.Repositories;
using Infrastructure.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ProblemRepository(AppDbContext db) : IProblemRepository
{
    private readonly AppDbContext _db = db;
    private static readonly int AcceptedProblemStatusId = 3;

    public async Task<ProblemModel?> GetProblemByIdAsync(
        Guid problemId,
        CancellationToken cancellationToken
    )
    {
        return await _db
            .Problems.Where(problem => problem.Id == problemId)
            .Select(problem => new ProblemModel()
            {
                Id = problem.Id,
                Slug = problem.Slug,
                Title = problem.Title,
                Question = problem.Question,
                Difficulty = problem.Difficulty,
                CreatedById = problem.CreatedById,
                CreatedBy =
                    problem.CreatedBy != null
                        ? new AccountModel()
                        {
                            Id = problem.CreatedBy.Id,
                            Username = problem.CreatedBy.Username,
                            ImageUrl = problem.CreatedBy.ImageUrl,
                            CreatedOn = problem.CreatedOn,
                        }
                        : null,
                Tags = problem.Tags.Select(tag => new TagModel()
                {
                    Id = tag.Id,
                    Value = tag.Value,
                }),
                ProblemSetups = problem
                    .ProblemSetups.Select(ps => new ProblemSetupModel
                    {
                        Id = ps.Id,
                        ProblemId = ps.ProblemId,
                        InitialCode = ps.InitialCode,
                        Version = ps.Version,
                        LanguageVersion = new LanguageVersion
                        {
                            Id = ps.LanguageVersion.Id,
                            ProgrammingLanguageId = ps.LanguageVersion.ProgrammingLanguageId,
                            ProgrammingLanguage =
                                ps.LanguageVersion.ProgrammingLanguage != null
                                    ? new ProgrammingLanguage
                                    {
                                        Id = ps.LanguageVersion.ProgrammingLanguage.Id,
                                        Name = ps.LanguageVersion.ProgrammingLanguage.Name,
                                        IsArchived = ps.LanguageVersion
                                            .ProgrammingLanguage
                                            .IsArchived,
                                        Versions = new List<LanguageVersion>(),
                                    }
                                    : null,
                            Version = ps.LanguageVersion.Version,
                        },
                    })
                    .ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ProblemModel?> GetProblemBySlugAsync(
        string slug,
        CancellationToken cancellationToken
    )
    {
        return !string.IsNullOrWhiteSpace(slug)
            ? await _db
                .Problems.Where(problem => problem.Slug == slug)
                .Select(problem => new ProblemModel()
                {
                    Id = problem.Id,
                    Slug = problem.Slug,
                    Title = problem.Title,
                    Question = problem.Question,
                    Difficulty = problem.Difficulty,
                    CreatedById = problem.CreatedById,
                    CreatedBy =
                        problem.CreatedBy != null
                            ? new AccountModel()
                            {
                                Id = problem.CreatedBy.Id,
                                Username = problem.CreatedBy.Username,
                                ImageUrl = problem.CreatedBy.ImageUrl,
                                CreatedOn = problem.CreatedOn,
                            }
                            : null,
                    Tags = problem.Tags.Select(tag => new TagModel()
                    {
                        Id = tag.Id,
                        Value = tag.Value,
                    }),
                    ProblemSetups = problem
                        .ProblemSetups.Select(ps => new ProblemSetupModel
                        {
                            Id = ps.Id,
                            ProblemId = ps.ProblemId,
                            InitialCode = ps.InitialCode,
                            Version = ps.Version,
                            LanguageVersion = new LanguageVersion
                            {
                                Id = ps.LanguageVersion.Id,
                                ProgrammingLanguageId = ps.LanguageVersion.ProgrammingLanguageId,
                                ProgrammingLanguage =
                                    ps.LanguageVersion.ProgrammingLanguage != null
                                        ? new ProgrammingLanguage
                                        {
                                            Id = ps.LanguageVersion.ProgrammingLanguage.Id,
                                            Name = ps.LanguageVersion.ProgrammingLanguage.Name,
                                            IsArchived = ps.LanguageVersion
                                                .ProgrammingLanguage
                                                .IsArchived,
                                            Versions = new List<LanguageVersion>(),
                                        }
                                        : null,
                                Version = ps.LanguageVersion.Version,
                            },
                        })
                        .ToList(),
                })
                .SingleOrDefaultAsync(cancellationToken)
            : null;
    }

    public async Task<PaginatedResult<ProblemModel>> GetProblemsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken
    )
    {
        int page = pagination.Page > 0 ? pagination.Page : 1;
        int size = pagination.Size > 0 ? pagination.Size : 10;

        var baseQuery = _db
            .Problems.Include(p => p.Tags)
            .Include(p => p.Status)
            .Where(p =>
                p.CreatedOn <= pagination.Timestamp
                && p.DeletedOn == null
                && p.StatusId == AcceptedProblemStatusId
            );

        var ordered =
            pagination.Direction == SortDirection.Asc
                ? baseQuery.OrderBy(p => p.CreatedOn).ThenBy(p => p.Id)
                : baseQuery.OrderByDescending(p => p.CreatedOn).ThenByDescending(p => p.Id);

        int total = await ordered.CountAsync(cancellationToken);

        var problems = await ordered
            .Skip((page - 1) * size)
            .Take(size)
            .Select(problem => new ProblemModel
            {
                Id = problem.Id,
                Title = problem.Title,
                Slug = problem.Slug,
                Question = problem.Question,
                Difficulty = problem.Difficulty,
                Tags = problem.Tags.Select(tag => new TagModel { Id = tag.Id, Value = tag.Value }),
                Version = problem.Version,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<ProblemModel>
        {
            Results = problems,
            Total = total,
            Page = page,
            Size = size,
        };
    }

    public async Task<ProblemSetupModel> GetProblemSetupAsync(
        Guid problemId,
        int languageVersionId,
        CancellationToken cancellationToken
    )
    {
        return await _db
            .ProblemSetups.Where(setup =>
                setup.ProblemId == problemId
                && setup.ProgrammingLanguageVersionId == languageVersionId
            )
            .Include(s => s.HarnessTemplate)
            .Select(ps => new ProblemSetupModel
            {
                Id = ps.Id,
                ProblemId = ps.ProblemId,
                InitialCode = ps.InitialCode,
                Version = ps.Version,
                FunctionName = ps.FunctionName,
                LanguageVersion = new LanguageVersion
                {
                    Id = ps.LanguageVersion.Id,
                    Version = ps.LanguageVersion.Version,
                    ProgrammingLanguageId = ps.LanguageVersion.ProgrammingLanguageId,
                    ProgrammingLanguage =
                        ps.LanguageVersion.ProgrammingLanguage != null
                            ? new ProgrammingLanguage
                            {
                                Id = ps.LanguageVersion.ProgrammingLanguage.Id,
                                Name = ps.LanguageVersion.ProgrammingLanguage.Name,
                                IsArchived = ps.LanguageVersion.ProgrammingLanguage.IsArchived,
                                Versions = new List<LanguageVersion>(),
                            }
                            : null,
                },
                HarnessTemplate =
                    ps.HarnessTemplate != null
                        ? new HarnessTemplate
                        {
                            Id = ps.HarnessTemplate.Id,
                            Template = ps.HarnessTemplate.Template,
                        }
                        : null,
                TestSuites = ps
                    .TestSuites.Select(ts => new TestSuiteModel
                    {
                        Id = ts.Id,
                        Name = ts.Name,
                        Description = ts.Description,
                        TestSuiteType = (TestSuiteType)ts.TestSuiteType.Id,
                        TestCases = ts
                            .TestCases.Select(tc => new TestCaseModel
                            {
                                Id = tc.Id,
                                Input = tc.IoPayload.Input,
                                ExpectedOutput = tc.IoPayload.ExpectedOutput,
                                TestCaseType = (TestCaseType)tc.TestCaseTypeId,
                            })
                            .ToList(),
                    })
                    .ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ProblemSetupModel?> GetProblemSetupAsync(
        int setupId,
        CancellationToken cancellationToken
    )
    {
        return await _db
            .ProblemSetups.Where(ps => ps.Id == setupId)
            .Include(ps => ps.HarnessTemplate)
            .Select(ps => new ProblemSetupModel
            {
                Id = ps.Id,
                ProblemId = ps.ProblemId,
                InitialCode = ps.InitialCode,
                Version = ps.Version,
                FunctionName = ps.FunctionName,
                LanguageVersion =
                    ps.LanguageVersion != null
                        ? new LanguageVersion
                        {
                            Id = ps.LanguageVersion.Id,
                            Version = ps.LanguageVersion.Version,
                            ProgrammingLanguageId = ps.LanguageVersion.ProgrammingLanguageId,
                            ProgrammingLanguage =
                                ps.LanguageVersion.ProgrammingLanguage != null
                                    ? new ProgrammingLanguage
                                    {
                                        Id = ps.LanguageVersion.ProgrammingLanguage.Id,
                                        Name = ps.LanguageVersion.ProgrammingLanguage.Name,
                                        IsArchived = ps.LanguageVersion
                                            .ProgrammingLanguage
                                            .IsArchived,
                                        Versions = new List<LanguageVersion>(),
                                    }
                                    : null,
                        }
                        : null,
                HarnessTemplate =
                    ps.HarnessTemplate != null
                        ? new HarnessTemplate
                        {
                            Id = ps.HarnessTemplate.Id,
                            Template = ps.HarnessTemplate.Template,
                        }
                        : null,
                TestSuites = ps
                    .TestSuites.Select(ts => new TestSuiteModel
                    {
                        Id = ts.Id,
                        Name = ts.Name,
                        Description = ts.Description,
                        TestSuiteType = (TestSuiteType)ts.TestSuiteType.Id,
                        TestCases = ts
                            .TestCases.Select(tc => new TestCaseModel
                            {
                                Id = tc.Id,
                                Input = tc.IoPayload.Input,
                                ExpectedOutput = tc.IoPayload.ExpectedOutput,
                                TestCaseType = (TestCaseType)tc.TestCaseTypeId,
                            })
                            .ToList(),
                    })
                    .ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}
