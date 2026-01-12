using ApplicationCore.Dtos.Submissions;

namespace ApplicationCore.Queries.Submissions.GetSubmission;

public sealed record GetSubmissionQuery(Guid Id) : IQuery<SubmissionDto>;
