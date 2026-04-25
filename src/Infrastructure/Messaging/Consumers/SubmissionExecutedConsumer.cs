using ApplicationCore.Domain.Submissions.Outboxes;
using ApplicationCore.Interfaces.Services;
using ApplicationCore.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Messaging.Consumers;

public sealed class SubmissionExecutedConsumer(IServiceScopeFactory serviceScopeFactory)
    : IConsumer<SubmissionExecutedMessage>
{
    public async Task Consume(ConsumeContext<SubmissionExecutedMessage> context)
    {
        var cancellationToken = context.CancellationToken;
        using var scope = serviceScopeFactory.CreateScope();

        var submissionAppService = scope.ServiceProvider.GetRequiredService<ISubmissionAppService>();
        var codeExecutionService = scope.ServiceProvider.GetRequiredService<ICodeExecutionService>();

        var outboxResults = await submissionAppService.GetSubmissionOutboxesAsync(cancellationToken);

        if (!outboxResults.IsSuccess || !outboxResults.Value.Any())
        {
            return;
        }

        var outboxes = outboxResults.Value
            .Where(outbox =>
                outbox.SubmissionId == context.Message.SubmissionId
                && outbox.Type == SubmissionOutboxType.PollExecution)
            .ToList();

        if (outboxes.Count == 0)
        {
            return;
        }

        var submissionResults = await codeExecutionService.GetSubmissionResultsAsync(
            outboxes.Select(o => o.Submission),
            cancellationToken
        );

        var outboxIds = outboxes.Select(outbox => outbox.Id).ToList();
        var now = DateTime.UtcNow;

        await submissionAppService.IncrementOutboxesCountAsync(outboxIds, now, cancellationToken);

        await submissionAppService.ProcessPollingSubmissionExecutionsAsync(
            submissionResults.Value,
            cancellationToken
        );
    }
}
