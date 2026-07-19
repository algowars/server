using Algowars.Application.Submissions.Dtos;

namespace Algowars.Application.Queries.Submissions.GetSubmissionStatus;

public sealed record GetSubmissionStatusQuery(Guid SubmissionId, Guid UserId) : IQuery<SubmissionStatusDto>;