using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Services
{
    public class AzureServiceBusPublisher : IEventPublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;
        private readonly ILogger<AzureServiceBusPublisher> _logger;

        public AzureServiceBusPublisher(
            IConfiguration configuration,
            ILogger<AzureServiceBusPublisher> logger)
        {
            _logger = logger;
            var connectionString = configuration["AzureServiceBus:ConnectionString"];
            var topicName = configuration["AzureServiceBus:TopicName"] ?? "school-events";

            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(topicName);
        }

        public async Task PublishAsync(
            string eventType,
            string payload,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new ServiceBusMessage(payload)
                {
                    ContentType = "application/json",
                    Subject = eventType,
                    MessageId = Guid.NewGuid().ToString()
                };

                // Add custom properties for filtering
                message.ApplicationProperties.Add("EventType", eventType);
                message.ApplicationProperties.Add("PublishedAt", DateTime.UtcNow);

                await _sender.SendMessageAsync(message, cancellationToken);

                _logger.LogInformation(
                    "Published event {EventType} to Azure Service Bus",
                    eventType
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish event {EventType} to Azure Service Bus",
                    eventType
                );
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            await _client.DisposeAsync();
        }
    }
}
