using ApplicationCore.Domain.Submissions;

namespace ApplicationCore.Queries.Submissions.GetSubmission;

public sealed record GetSubmissionQuery(Guid SubmissionId) : IQuery<SubmissionModel>;
