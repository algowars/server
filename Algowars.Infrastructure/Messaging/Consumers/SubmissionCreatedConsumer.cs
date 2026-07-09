using Algowars.Application.Messaging.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Algowars.Infrastructure.Messaging.Consumers;

public sealed partial class SubmissionCreatedConsumer(
    ILogger<SubmissionCreatedConsumer> logger
) : IConsumer<SubmissionCreatedMessage>
{
    public Task Consume(ConsumeContext<SubmissionCreatedMessage> context)
    {
        LogSubmissionReceived(context.Message.SubmissionId);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Received SubmissionCreatedMessage for submission {SubmissionId}")]
    private partial void LogSubmissionReceived(Guid SubmissionId);
}