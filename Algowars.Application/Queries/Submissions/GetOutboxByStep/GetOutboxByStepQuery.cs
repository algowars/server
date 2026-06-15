using Algowars.Application.Dtos.Submissions;
using Algowars.Domain.Submissions.Outbox.Enums;

namespace Algowars.Application.Queries.Submissions.GetOutboxByStep;

public sealed record GetOutboxByStepQuery(SubmissionOutboxStep Step, int BatchSize = 50)
    : IQuery<IReadOnlyList<SubmissionOutboxDto>>;
