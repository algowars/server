using Algowars.Application.Pagination;
using Algowars.Application.Submissions.Dtos;

namespace Algowars.Application.Queries.Submissions.GetSubmissionsByProblemSlug;

internal sealed record GetSubmissionsByProblemSlugQuery(string ProblemSlug, PaginationRequest PaginationRequest, Guid? UserId, bool IncludeAllSubmissions) : IQuery<PageResult<ProblemSubmissionDto>>;