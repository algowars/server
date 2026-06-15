using Algowars.Domain.Submissions.Outbox;
using Algowars.Domain.Submissions.Outbox.Enums;
using Ardalis.Result;
using MediatR;

namespace Algowars.Application.Commands.Submissions.Outbox;

internal sealed class CompleteOutboxStepHandler(
    ISubmissionOutboxRepository outboxRepository,
    ISender sender)
    : ICommandHandler<CompleteOutboxStepCommand>
{
    private static readonly SubmissionOutboxStep[] _orderedSteps =
    [
        SubmissionOutboxStep.Execute,
        SubmissionOutboxStep.PollExecution,
        SubmissionOutboxStep.Evaluate,
        SubmissionOutboxStep.EvaluationPoll,
    ];

    public async Task<Result> Handle(CompleteOutboxStepCommand request, CancellationToken cancellationToken)
    {
        var outbox = await outboxRepository.FindByIdAsync(request.OutboxId, cancellationToken);
        if (outbox is null)
            return Result.NotFound();

        outbox.Complete(DateTime.UtcNow);
        await outboxRepository.UpdateAsync(outbox, cancellationToken);

        var nextStep = NextStep(outbox.Step);
        if (nextStep is not null)
            await sender.Send(new BeginOutboxStepCommand(outbox.SubmissionId, nextStep.Value), cancellationToken);

        return Result.Success();
    }

    private static SubmissionOutboxStep? NextStep(SubmissionOutboxStep current)
    {
        int index = Array.IndexOf(_orderedSteps, current);
        return index >= 0 && index < _orderedSteps.Length - 1
            ? _orderedSteps[index + 1]
            : null;
    }
}
