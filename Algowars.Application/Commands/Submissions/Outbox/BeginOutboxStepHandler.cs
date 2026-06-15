using Algowars.Domain.Submissions.Outbox;
using Algowars.Domain.Submissions.Outbox.Enums;
using Ardalis.Result;

namespace Algowars.Application.Commands.Submissions.Outbox;

internal sealed class BeginOutboxStepHandler(ISubmissionOutboxRepository outboxRepository)
    : ICommandHandler<BeginOutboxStepCommand, Guid>
{
    public async Task<Result<Guid>> Handle(BeginOutboxStepCommand request, CancellationToken cancellationToken)
    {
        var outbox = SubmissionOutbox.CreateForStep(request.SubmissionId, request.Step);
        await outboxRepository.AddAsync(outbox, cancellationToken);
        return Result.Success(outbox.Id);
    }
}
