using Algowars.Application.Messaging;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Algowars.Infrastructure.Messaging.Consumers;

internal sealed partial class SubmissionCreatedConsumer(
    ILogger<SubmissionCreatedConsumer> logger) : IConsumer<SubmissionCreatedMessage>
{
    public Task Consume(ConsumeContext<SubmissionCreatedMessage> context)
    {
        LogSubmissionReceived(context.Message.SubmissionId);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Submission {SubmissionId} received — Execute outbox step seeded")]
    private partial void LogSubmissionReceived(Guid submissionId);
}
