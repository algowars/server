using Algowars.Application.Dtos.Submissions;
using Algowars.Domain.Submissions.Outbox;
using Ardalis.Result;

namespace Algowars.Application.Queries.Submissions.GetOutboxByStep;

internal sealed class GetOutboxByStepHandler(ISubmissionOutboxRepository outboxRepository)
    : IQueryHandler<GetOutboxByStepQuery, IReadOnlyList<SubmissionOutboxDto>>
{
    public async Task<Result<IReadOnlyList<SubmissionOutboxDto>>> Handle(
        GetOutboxByStepQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await outboxRepository.GetPendingByStepAsync(request.Step, request.BatchSize, cancellationToken);

        var dtos = rows.Select(o => new SubmissionOutboxDto
        {
            Id = o.Id,
            SubmissionId = o.SubmissionId,
            Step = o.Step.ToString(),
            Status = o.Status.ToString(),
            AttemptCount = o.AttemptCount,
            MaxAttempts = o.MaxAttempts,
            CreatedAt = o.CreatedAt,
            LastAttemptAt = o.LastAttemptAt,
            CompletedAt = o.CompletedAt,
            LastError = o.LastError,
        }).ToList();

        return Result.Success<IReadOnlyList<SubmissionOutboxDto>>(dtos);
    }
}
