using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging.Consumers;

/// <summary>
/// Stage 4: Final stage — increments attempt count and sets FinalizedOn
/// on the outbox, completing the submission pipeline.
/// </summary>
public sealed class SubmissionEvaluationPollConsumer(IServiceScopeFactory serviceScopeFactory)
    : IConsumer<SubmissionEvaluationPollMessage>
{
    public async Task Consume(ConsumeContext<SubmissionEvaluationPollMessage> context)
    {
        var cancellationToken = context.CancellationToken;
        using var scope = serviceScopeFactory.CreateScope();

        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outbox = outboxResults.Value.FirstOrDefault(o =>
            o.SubmissionId == context.Message.SubmissionId
            && o.Type == SubmissionOutboxType.EvaluationPoll);

        if (outbox is null)
        {
            return;
        }

        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync([outbox.Id], now, cancellationToken);
        await submissionAppService.FinalizeEvaluationAsync([outbox.Id], now, cancellationToken);
    }
}
