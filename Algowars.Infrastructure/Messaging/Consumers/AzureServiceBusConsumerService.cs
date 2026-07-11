using System.Text.Json;
using Algowars.Application.Messaging.Messages;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algowars.Infrastructure.Messaging.Consumers;

internal sealed partial class AzureServiceBusConsumerService(
    ServiceBusClient client,
    ILogger<AzureServiceBusConsumerService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = client.CreateProcessor(QueueNames.ForType<SubmissionCreatedMessage>());

        processor.ProcessMessageAsync += async args =>
        {
            var message = JsonSerializer.Deserialize<SubmissionCreatedMessage>(args.Message.Body);
            if (message is not null)
                LogSubmissionReceived(message.SubmissionId);
            await args.CompleteMessageAsync(args.Message);
        };

        processor.ProcessErrorAsync += args =>
        {
            LogProcessingError(args.Exception);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);

        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { }

        await processor.StopProcessingAsync();
        await processor.DisposeAsync();
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Received SubmissionCreatedMessage for submission {SubmissionId}")]
    private partial void LogSubmissionReceived(Guid SubmissionId);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Error processing message from Azure Service Bus")]
    private partial void LogProcessingError(Exception ex);
}
