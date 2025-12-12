// Infrastructure/EventBus/AzureServiceBusEventBus.cs
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.EventBus
{
    /// <summary>
    /// Production-ready Azure Service Bus event publisher
    /// Implements connection pooling and proper lifecycle management
    /// </summary>
    public class AzureServiceBusEventBus : IEventBus, IAsyncDisposable
    {
        private readonly ILogger<AzureServiceBusEventBus> _logger;
        private readonly ServiceBusClient _client;
        private readonly string _topicPrefix;
        private readonly JsonSerializerOptions _jsonOptions;

        // ✅ CRITICAL: Reuse senders for performance
        private readonly ConcurrentDictionary<string, ServiceBusSender> _senders;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public AzureServiceBusEventBus(
            ILogger<AzureServiceBusEventBus> logger,
            IConfiguration configuration,
            ServiceBusClient serviceBusClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
            _topicPrefix = configuration["AzureServiceBus:TopicPrefix"] ?? "default";
            _senders = new ConcurrentDictionary<string, ServiceBusSender>();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
            where T : IEvent
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var eventType = typeof(T).Name;
            var topicName = GetTopicName(eventType);

            try
            {
                // ✅ Get or create sender (reused for performance)
                var sender = await GetOrCreateSenderAsync(topicName);

                // Serialize event
                var body = JsonSerializer.Serialize(@event, _jsonOptions);

                // Create message with metadata
                var message = new ServiceBusMessage(body)
                {
                    ContentType = "application/json",
                    Subject = eventType,
                    MessageId = @event.EventId.ToString(),
                    CorrelationId = @event.EventId.ToString(),
                    // ✅ Add TTL for message expiration (24 hours)
                    TimeToLive = TimeSpan.FromHours(24)
                };

                // Add custom properties for filtering
                message.ApplicationProperties.Add("EventType", eventType);
                message.ApplicationProperties.Add("OccurredOn", @event.OccurredOn);
                message.ApplicationProperties.Add("Source", "SchoolManagement");
                message.ApplicationProperties.Add("Environment",
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");

                // ✅ Send with retry built into Azure SDK
                await sender.SendMessageAsync(message, cancellationToken);

                _logger.LogInformation(
                    "Published event {EventType} with ID {EventId} to topic {TopicName}",
                    eventType,
                    @event.EventId,
                    topicName);
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
            {
                _logger.LogError(
                    ex,
                    "Topic {TopicName} not found. Ensure it exists in Azure Service Bus",
                    topicName);
                throw;
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.QuotaExceeded)
            {
                _logger.LogError(
                    ex,
                    "Service Bus quota exceeded for topic {TopicName}. Check namespace limits",
                    topicName);
                throw;
            }
            catch (ServiceBusException ex)
            {
                _logger.LogError(
                    ex,
                    "Service Bus error publishing event {EventType} to {TopicName}. Reason: {Reason}",
                    eventType,
                    topicName,
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
            where T : IEvent
        {
            if (events == null || !events.Any())
                return;

            var eventType = typeof(T).Name;
            var topicName = GetTopicName(eventType);
            var eventList = events.ToList();

            try
            {
                var sender = await GetOrCreateSenderAsync(topicName);

                // ✅ Process in batches to handle large volumes
                var batchCount = 0;
                var totalSent = 0;

                foreach (var batch in eventList.Chunk(100)) // Process 100 at a time
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
                        message.ApplicationProperties.Add("OccurredOn", @event.OccurredOn);
                        message.ApplicationProperties.Add("Source", "SchoolManagement");

                        if (!messageBatch.TryAddMessage(message))
                        {
                            // Current batch is full, send it
                            await sender.SendMessagesAsync(messageBatch, cancellationToken);
                            totalSent += messageBatch.Count;
                            batchCount++;

                            _logger.LogDebug(
                                "Sent batch {BatchNumber} with {Count} messages",
                                batchCount,
                                messageBatch.Count);

                            // Try adding to new batch (should succeed now)
                            if (!messageBatch.TryAddMessage(message))
                            {
                                _logger.LogWarning(
                                    "Message too large for batch. EventId: {EventId}",
                                    @event.EventId);
                            }
                        }
                    }

                    // Send remaining messages
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
                    topicName);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error publishing batch of {Count} events to {TopicName}",
                    eventList.Count,
                    topicName);
                throw;
            }
        }

        /// <summary>
        /// Get or create sender with thread-safe lazy initialization
        /// Senders are cached and reused for better performance
        /// </summary>
        private async Task<ServiceBusSender> GetOrCreateSenderAsync(string topicName)
        {
            if (_senders.TryGetValue(topicName, out var existingSender))
            {
                return existingSender;
            }

            await _semaphore.WaitAsync();
            try
            {
                // Double-check after acquiring lock
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

        private string GetTopicName(string eventType)
        {
            // Convert to kebab-case: UserLoggedInEvent -> user-logged-in-event
            var topicName = string.Concat(eventType.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? "-" + x.ToString() : x.ToString())).ToLower();

            return $"{_topicPrefix}-{topicName}";
        }

        public async ValueTask DisposeAsync()
        {
            _logger.LogInformation("Disposing Azure Service Bus Event Bus...");

            // Dispose all senders
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

            // Dispose client
            if (_client != null)
            {
                await _client.DisposeAsync();
            }

            _semaphore?.Dispose();

            _logger.LogInformation("Azure Service Bus Event Bus disposed");
        }
    }
}
