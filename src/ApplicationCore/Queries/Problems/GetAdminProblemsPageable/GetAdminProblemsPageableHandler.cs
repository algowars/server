using ApplicationCore.Common.Pagination;
using ApplicationCore.Dtos.Accounts;
using ApplicationCore.Dtos.Languages;
using ApplicationCore.Dtos.Problems.Admin;
using ApplicationCore.Interfaces.Repositories;
using Ardalis.Result;
using System.Linq;

namespace ApplicationCore.Queries.Problems.GetAdminProblemsPageable;

public class GetAdminProblemsPageableHandler(IProblemRepository repository)
    : IQueryHandler<GetAdminProblemsPageableQuery, PaginatedResult<AdminProblemDto>>
{
    public async Task<Result<PaginatedResult<AdminProblemDto>>> Handle(
        GetAdminProblemsPageableQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var problemsPage = await repository.GetProblemsAsync(
                request.Pagination,
                cancellationToken
            );

            var dtoItems = problemsPage.Results.Select(problem => new AdminProblemDto(
                Id: problem.Id,
                Title: problem.Title,
                Status: problem.Status,
                CreatedOn: problem.CreatedOn,
                ProgrammingLanguages: problem
                    .ProblemSetups?.Select(setup => setup.LanguageVersion?.ProgrammingLanguage)
                    .Where(lang => lang is not null)
                    .DistinctBy(lang => lang!.Id)
                    .Select(language => new ProgrammingLanguageDto
                    {
                        Id = language!.Id,
                        Name = language.Name,
                    })
                    .ToList()
                    ?? [],
                CreatedBy: problem.CreatedBy is not null
                    ? new AccountDto
                    {
                        Id = problem.CreatedBy.Id,
                        Username = problem.CreatedBy.Username,
                        CreatedOn = problem.CreatedOn,
                    }
                    : null
            ));

            return Result.Success(
                new PaginatedResult<AdminProblemDto>
                {
                    Results = dtoItems.ToList(),
                    Page = problemsPage.Page,
                    Size = problemsPage.Size,
                    Total = problemsPage.Total,
                }
            );
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}