using ApplicationCore.Dtos.Submissions;

namespace ApplicationCore.Queries.Submissions.GetSubmissionStatus;

public sealed record GetSubmissionStatusQuery(Guid SubmissionId) : IQuery<SubmissionStatusDto>;
