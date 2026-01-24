using ApplicationCore.Common.Pagination;
using ApplicationCore.Domain.Submissions;

namespace ApplicationCore.Queries.Submissions.GetSubmissions;

public sealed record GetSubmissionsQuery(Guid ProblemId, PaginationRequest PaginationRequest)
    : IQuery<IEnumerable<SubmissionModel>>;
