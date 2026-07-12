using Algowars.Application.Messaging;

namespace Algowars.Application.Messaging.Messages;

public sealed record SubmissionCreatedMessage(Guid SubmissionId) : IMessage
{
    public static string QueueName => "submission-created";
}
