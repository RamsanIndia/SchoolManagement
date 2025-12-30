// Infrastructure/EventBus/AzureServiceBusEventBus.cs
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolManagement.Domain.Common;
using SchoolManagement.Infrastructure.EventBus;
using System.Collections.Concurrent;
using System.Text.Json;

public class AzureServiceBusEventBus : IEventBus, IAsyncDisposable
{
    private readonly ILogger<AzureServiceBusEventBus> _logger;
    private readonly ServiceBusClient _client;
    private readonly string _topicName; // Changed from _topicPrefix
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public AzureServiceBusEventBus(
        ILogger<AzureServiceBusEventBus> logger,
        IConfiguration configuration,
        ServiceBusClient serviceBusClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));

        // Use single topic name from configuration
        _topicName = configuration["AzureServiceBus:TopicName"]
            ?? throw new InvalidOperationException("AzureServiceBus:TopicName configuration is required");

        _senders = new ConcurrentDictionary<string, ServiceBusSender>();

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : IDomainEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(T).Name;

        try
        {
            // Get sender for the single topic
            var sender = await GetOrCreateSenderAsync(_topicName);

            // Serialize event
            var body = JsonSerializer.Serialize(@event, _jsonOptions);

            // Create message with metadata
            var message = new ServiceBusMessage(body)
            {
                ContentType = "application/json",
                Subject = eventType, // Use Subject for filtering
                MessageId = @event.EventId.ToString(),
                CorrelationId = @event.EventId.ToString(),
                TimeToLive = TimeSpan.FromHours(24)
            };

            // Add properties for subscription filtering
            message.ApplicationProperties.Add("EventType", eventType);
            message.ApplicationProperties.Add("FullEventType", typeof(T).AssemblyQualifiedName);
            message.ApplicationProperties.Add("OccurredOn", @event.OccurredOn);
            message.ApplicationProperties.Add("Source", "SchoolManagement");
            message.ApplicationProperties.Add("Environment",
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");

            // Send message
            await sender.SendMessageAsync(message, cancellationToken);

            _logger.LogInformation(
                "Published event {EventType} with ID {EventId} to topic {TopicName}",
                eventType,
                @event.EventId,
                _topicName);
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            _logger.LogError(
                ex,
                "Topic {TopicName} not found. Ensure it exists in Azure Service Bus namespace",
                _topicName);
            throw;
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.QuotaExceeded)
        {
            _logger.LogError(
                ex,
                "Service Bus quota exceeded for topic {TopicName}. Check namespace limits",
                _topicName);
            throw;
        }
        catch (ServiceBusException ex)
        {
            _logger.LogError(
                ex,
                "Service Bus error publishing event {EventType} to {TopicName}. Reason: {Reason}",
                eventType,
                _topicName,
                ex.Reason);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error publishing event {EventType} with ID {EventId}",
                eventType,
                @event.EventId);
            throw;
        }
    }

    public async Task PublishBatchAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default)
        where T : IDomainEvent
    {
        if (events == null || !events.Any())
            return;

        var eventType = typeof(T).Name;
        var eventList = events.ToList();

        try
        {
            var sender = await GetOrCreateSenderAsync(_topicName);

            var batchCount = 0;
            var totalSent = 0;

            foreach (var batch in eventList.Chunk(100))
            {
                using var messageBatch = await sender.CreateMessageBatchAsync(cancellationToken);

                foreach (var @event in batch)
                {
                    var body = JsonSerializer.Serialize(@event, _jsonOptions);
                    var message = new ServiceBusMessage(body)
                    {
                        ContentType = "application/json",
                        Subject = eventType,
                        MessageId = @event.EventId.ToString(),
                        CorrelationId = @event.EventId.ToString(),
                        TimeToLive = TimeSpan.FromHours(24)
                    };

                    message.ApplicationProperties.Add("EventType", eventType);
                    message.ApplicationProperties.Add("FullEventType", typeof(T).AssemblyQualifiedName);
                    message.ApplicationProperties.Add("OccurredOn", @event.OccurredOn);
                    message.ApplicationProperties.Add("Source", "SchoolManagement");

                    if (!messageBatch.TryAddMessage(message))
                    {
                        await sender.SendMessagesAsync(messageBatch, cancellationToken);
                        totalSent += messageBatch.Count;
                        batchCount++;

                        _logger.LogDebug(
                            "Sent batch {BatchNumber} with {Count} messages",
                            batchCount,
                            messageBatch.Count);

                        if (!messageBatch.TryAddMessage(message))
                        {
                            _logger.LogWarning(
                                "Message too large for batch. EventId: {EventId}",
                                @event.EventId);
                        }
                    }
                }

                if (messageBatch.Count > 0)
                {
                    await sender.SendMessagesAsync(messageBatch, cancellationToken);
                    totalSent += messageBatch.Count;
                    batchCount++;
                }
            }

            _logger.LogInformation(
                "Published {TotalCount} events of type {EventType} in {BatchCount} batches to {TopicName}",
                totalSent,
                eventType,
                batchCount,
                _topicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error publishing batch of {Count} events to {TopicName}",
                eventList.Count,
                _topicName);
            throw;
        }
    }

    private async Task<ServiceBusSender> GetOrCreateSenderAsync(string topicName)
    {
        if (_senders.TryGetValue(topicName, out var existingSender))
        {
            return existingSender;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_senders.TryGetValue(topicName, out existingSender))
            {
                return existingSender;
            }

            var newSender = _client.CreateSender(topicName);
            _senders.TryAdd(topicName, newSender);

            _logger.LogInformation("Created new sender for topic: {TopicName}", topicName);

            return newSender;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Remove the GetTopicName method - no longer needed

    public async ValueTask DisposeAsync()
    {
        _logger.LogInformation("Disposing Azure Service Bus Event Bus...");

        foreach (var kvp in _senders)
        {
            try
            {
                await kvp.Value.DisposeAsync();
                _logger.LogDebug("Disposed sender for topic: {TopicName}", kvp.Key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing sender for topic: {TopicName}", kvp.Key);
            }
        }
        _senders.Clear();

        if (_client != null)
        {
            await _client.DisposeAsync();
        }

        _semaphore?.Dispose();

        _logger.LogInformation("Azure Service Bus Event Bus disposed");
    }
}