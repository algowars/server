using ApplicationCore.Commands.Problem.CreateProblem;
using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Problems.ProblemSetups;
using ApplicationCore.Dtos.Languages;
using ApplicationCore.Dtos.Problems;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Queries.Problems.GetAvailableLanguages;
using ApplicationCore.Queries.Problems.GetProblemBySlug;
using ApplicationCore.Queries.Problems.GetProblemSetup;
using ApplicationCore.Queries.Problems.GetProblemSetupsForExecution;
using ApplicationCore.Queries.Problems.GetProblemsPageable;
using Ardalis.Result;
using MediatR;

namespace ApplicationCore.Services;

public sealed class ProblemAppService(IMediator mediator) : IProblemAppService
{
    public Task<Result<Guid>> CreateProblemAsync(
        CreateProblemDto createProblemDto,
        Guid createdById,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateProblemCommand(
            Title: createProblemDto.Title,
            Question: createProblemDto.Question,
            EstimatedDifficulty: createProblemDto.EstimatedDifficulty,
            Tags: createProblemDto.Tags,
            CreatedById: createdById
        );

        return mediator.Send(command, cancellationToken);
    }

    public Task<Result<IEnumerable<ProgrammingLanguageDto>>> GetAvailableLanguagesAsync(
        CancellationToken cancellationToken
    )
    {
        var query = new GetAvailableLanguagesQuery();

        return mediator.Send(query, cancellationToken);
    }

    public async Task<Result<ProblemDto>> GetProblemBySlugAsync(
        string slug,
        CancellationToken cancellationToken
    )
    {
        var query = new GetProblemBySlugQuery(slug);

        return await mediator.Send(query, cancellationToken);
    }

    public async Task<Result<ProblemSetupDto>> GetProblemSetupAsync(
        Guid problemId,
        int languageVersionId,
        CancellationToken cancellationToken
    )
    {
        var query = new GetProblemSetupQuery(problemId, languageVersionId);

        return await mediator.Send(query, cancellationToken);
    }

    public async Task<Result<PaginatedResult<ProblemDto>>> GetProblemsPaginatedAsync(
        int pageNumber,
        int pageSize,
        DateTime timestamp,
        CancellationToken cancellationToken
    )
    {
        var pagination = new PaginationRequest
        {
            Page = pageNumber,
            Size = pageSize,
            Timestamp = timestamp,
        };
        var query = new GetProblemsPageableQuery(pagination);

        return await mediator.Send(query, cancellationToken);
    }

    public async Task<Result<PaginatedResult<ProblemDto>>> GetProblemsPaginatedAsync(
        int pageNumber,
        int pageSize,
        DateTime timestamp,
        string query,
        CancellationToken cancellationToken
    )
    {
        var pagination = new PaginationRequest
        {
            Page = pageNumber,
            Size = pageSize,
            Timestamp = timestamp,
            Query = query,
        };

        var pageableQuery = new GetProblemsPageableQuery(pagination);

        return await mediator.Send(pageableQuery, cancellationToken);
    }

    public Task<Result<IEnumerable<ProblemSetupModel>>> GetProblemSetupsForExecutionAsync(
        IEnumerable<int> setupIds,
        CancellationToken cancellationToken
    )
    {
        var query = new GetProblemSetupsForExecutionQuery(setupIds);

        return mediator.Send(query, cancellationToken);
    }
}