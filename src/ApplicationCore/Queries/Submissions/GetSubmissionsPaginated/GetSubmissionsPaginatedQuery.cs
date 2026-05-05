using ApplicationCore.Common.Pagination;
using ApplicationCore.Dtos.Submissions;

namespace ApplicationCore.Queries.Submissions.GetSubmissionsPaginated;

public sealed record GetSubmissionsPaginatedQuery(
    Guid ProblemId,
    PaginationRequest Pagination,
    Guid? FilterByUserId = null,
    bool AcceptedOnly = true
) : IQuery<PaginatedResult<SubmissionDto>>;