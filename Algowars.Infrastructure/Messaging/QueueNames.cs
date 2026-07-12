using Algowars.Application.Messaging;

namespace Algowars.Infrastructure.Messaging;

internal static class QueueNames
{
    public static string ForType<T>() where T : IMessage => T.QueueName;
}
