namespace Algowars.Infrastructure.Messaging;

internal static class QueueNames
{
    public static string ForType<T>() =>
        typeof(T).Name.ToLowerInvariant().Replace("message", "-message");
}
