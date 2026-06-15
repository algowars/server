using Algowars.Application.Dtos.Submissions;

namespace Algowars.Application.Queries.Submissions.GetSubmissionStatus;

public sealed record GetSubmissionStatusQuery(Guid SubmissionId) : IQuery<SubmissionStatusDto>;
