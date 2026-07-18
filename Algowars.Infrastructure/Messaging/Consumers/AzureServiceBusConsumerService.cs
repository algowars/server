using System.Text.Json;
using Algowars.Application.Messaging;
using Algowars.Application.Messaging.Messages;
using Algowars.Infrastructure.Jobs.Submissions;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Algowars.Infrastructure.Messaging.Consumers;

internal sealed partial class AzureServiceBusConsumerService(
    ServiceBusClient client,
    IServiceScopeFactory scopeFactory,
    ILogger<AzureServiceBusConsumerService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var createdProcessor = client.CreateProcessor(QueueNames.ForType<SubmissionCreatedMessage>());
        var continuationProcessor = client.CreateProcessor(QueueNames.ForType<SubmissionJobContinuationMessage>());

        WireUp<SubmissionCreatedMessage>(createdProcessor, m => m.SubmissionId);
        WireUp<SubmissionJobContinuationMessage>(continuationProcessor, m => m.SubmissionId);

        await Task.WhenAll(
            createdProcessor.StartProcessingAsync(stoppingToken),
            continuationProcessor.StartProcessingAsync(stoppingToken));

        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { }

        await createdProcessor.StopProcessingAsync();
        await continuationProcessor.StopProcessingAsync();
        await createdProcessor.DisposeAsync();
        await continuationProcessor.DisposeAsync();
    }

    /// <summary>
    /// Wires up receive/error handlers for a processor: deserializes the message, dispatches
    /// it to <see cref="SubmissionJobProcessorService.RunForSubmissionAsync"/>, and completes
    /// the message only after processing succeeds (or logs and dead-letters on failure).
    /// </summary>
    private void WireUp<TMessage>(ServiceBusProcessor processor, Func<TMessage, Guid> getSubmissionId)
        where TMessage : IMessage
    {
        processor.ProcessMessageAsync += async args =>
        {
            TMessage? message;
            try
            {
                message = JsonSerializer.Deserialize<TMessage>(args.Message.Body);
            }
            catch (JsonException ex)
            {
                LogProcessingError(ex);
                await args.DeadLetterMessageAsync(args.Message, deadLetterReason: "DeserializationFailed");
                return;
            }

            if (message is null)
            {
                await args.DeadLetterMessageAsync(args.Message, deadLetterReason: "NullMessage");
                return;
            }

            Guid submissionId = getSubmissionId(message);
            LogSubmissionReceived(typeof(TMessage).Name, submissionId);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var processorService = scope.ServiceProvider.GetRequiredService<SubmissionJobProcessorService>();
                await processorService.RunForSubmissionAsync(submissionId, args.CancellationToken);
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                LogProcessingError(ex);
                await args.AbandonMessageAsync(args.Message);
            }
        };

        processor.ProcessErrorAsync += async args =>
        {
            if (args.Exception is ServiceBusException { Reason: ServiceBusFailureReason.MessagingEntityNotFound })
            {
                LogQueueNotFound(args.EntityPath);
                await processor.StopProcessingAsync();
                return;
            }

            LogProcessingError(args.Exception);
        };
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Received {MessageType} for submission {SubmissionId}")]
    private partial void LogSubmissionReceived(string messageType, Guid submissionId);

    [LoggerMessage(Level = LogLevel.Critical,
        Message = "Azure Service Bus queue '{QueueName}' not found — consumer stopped. Check queue name configuration.")]
    private partial void LogQueueNotFound(string QueueName);

    [LoggerMessage(Level = LogLevel.Error,
        Message = "Error processing message from Azure Service Bus")]
    private partial void LogProcessingError(Exception ex);
}