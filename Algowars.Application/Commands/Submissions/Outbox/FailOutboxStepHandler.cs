using Algowars.Domain.Submissions.Outbox;
using Ardalis.Result;

namespace Algowars.Application.Commands.Submissions.Outbox;

internal sealed class FailOutboxStepHandler(ISubmissionOutboxRepository outboxRepository)
    : ICommandHandler<FailOutboxStepCommand>
{
    public async Task<Result> Handle(FailOutboxStepCommand request, CancellationToken cancellationToken)
    {
        var outbox = await outboxRepository.FindByIdAsync(request.OutboxId, cancellationToken);
        if (outbox is null)
            return Result.NotFound();

        outbox.RecordFailure(request.Error, DateTime.UtcNow);
        await outboxRepository.UpdateAsync(outbox, cancellationToken);
        return Result.Success();
    }
}
