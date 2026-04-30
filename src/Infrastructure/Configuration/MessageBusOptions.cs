namespace Infrastructure.Configuration;

public sealed class MessageBusOptions
{
    public const string SectionName = "MessageBus";

    public string Transport { get; init; } = "RabbitMQ";

    public RabbitMqOptions RabbitMQ { get; init; } = new();

    public AzureServiceBusOptions AzureServiceBus { get; init; } = new();
}

public sealed class RabbitMqOptions
{
    public string Host { get; init; } = "localhost";
    public string VirtualHost { get; init; } = "/";
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
}

public sealed class AzureServiceBusOptions
{
    public string ConnectionString { get; init; } = string.Empty;
}