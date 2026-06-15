using Algowars.Domain.Submissions.Outbox;
using Ardalis.Result;

namespace Algowars.Application.Commands.Submissions.Outbox;

internal sealed class RecordOutboxAttemptHandler(ISubmissionOutboxRepository outboxRepository)
    : ICommandHandler<RecordOutboxAttemptCommand>
{
    public async Task<Result> Handle(RecordOutboxAttemptCommand request, CancellationToken cancellationToken)
    {
        var outbox = await outboxRepository.FindByIdAsync(request.OutboxId, cancellationToken);
        if (outbox is null)
            return Result.NotFound();

        outbox.RecordAttempt(DateTime.UtcNow);
        await outboxRepository.UpdateAsync(outbox, cancellationToken);
        return Result.Success();
    }
}
