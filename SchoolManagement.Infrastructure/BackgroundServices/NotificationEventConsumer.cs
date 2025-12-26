using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchoolManagement.Application.DTOs;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Domain.Enums;
using SchoolManagement.Domain.Events;
using SchoolManagement.Infrastructure.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.BackgroundServices
{
    public class NotificationEventConsumer : BackgroundService
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationEventConsumer> _logger;
        private readonly IEventRouter _eventRouter;
        private readonly string _topicName;
        private readonly string _subscriptionName;
        private ServiceBusProcessor _processor;

        public NotificationEventConsumer(
            ServiceBusClient serviceBusClient,
            IServiceProvider serviceProvider,
            IEventRouter eventRouter,
            ILogger<NotificationEventConsumer> logger,
            IConfiguration configuration) // ✅ Add configuration
        {
            _serviceBusClient = serviceBusClient;
            _serviceProvider = serviceProvider;
            _eventRouter = eventRouter;
            _logger = logger;

            // ✅ Read from configuration
            _topicName = configuration["AzureServiceBus:TopicName"] ?? "schoolmanagement-events";
            _subscriptionName = configuration["AzureServiceBus:NotificationSubscription"] ?? "notification-consumer";

            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            // Register handler for NotificationCreatedEvent
            _eventRouter.RegisterHandler<NotificationCreatedEvent>(
                async (@event, sp, ct) => await HandleNotificationCreatedAsync(@event, sp, ct));

            // Register handler for NotificationSentEvent
            _eventRouter.RegisterHandler<NotificationSentEvent>(
                async (@event, sp, ct) => await HandleNotificationSentAsync(@event, sp, ct));

            // Register handler for NotificationDeliveredEvent
            _eventRouter.RegisterHandler<NotificationDeliveredEvent>(
                async (@event, sp, ct) => await HandleNotificationDeliveredAsync(@event, sp, ct));

            // Register handler for NotificationFailedEvent
            _eventRouter.RegisterHandler<NotificationFailedEvent>(
                async (@event, sp, ct) => await HandleNotificationFailedAsync(@event, sp, ct));

            _logger.LogInformation("All event handlers registered successfully");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _processor = _serviceBusClient.CreateProcessor(
                    _topicName,           
                    _subscriptionName,    
                    new ServiceBusProcessorOptions
                    {
                        AutoCompleteMessages = false,
                        MaxConcurrentCalls = 10,
                        PrefetchCount = 10,
                        ReceiveMode = ServiceBusReceiveMode.PeekLock
                    });

                _processor.ProcessMessageAsync += ProcessMessageAsync;
                _processor.ProcessErrorAsync += ProcessErrorAsync;

                await _processor.StartProcessingAsync(stoppingToken);

                _logger.LogInformation(
                    "Notification Event Consumer started. Listening to {TopicName}/{SubscriptionName}",
                    _topicName,
                    _subscriptionName);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Failed to start Notification Event Consumer for {TopicName}/{SubscriptionName}. " +
                    "Ensure the topic and subscription exist in Azure Service Bus.",
                    _topicName,
                    _subscriptionName);
                throw;
            }
        }

        private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            using var scope = _serviceProvider.CreateScope();

            try
            {
                var eventType = args.Message.ApplicationProperties["EventType"].ToString();
                var eventData = args.Message.Body.ToString();

                _logger.LogInformation("Processing event: {EventType}", eventType);

                // Dynamic routing - no switch/case needed!
                await _eventRouter.RouteAsync(eventType, eventData, scope.ServiceProvider, args.CancellationToken);

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event");

                if (args.Message.DeliveryCount >= 3)
                {
                    await args.DeadLetterMessageAsync(args.Message, "ProcessingFailed", ex.Message);
                }
                else
                {
                    await args.AbandonMessageAsync(args.Message);
                }
            }
        }

        private async Task HandleNotificationCreatedAsync(
            NotificationCreatedEvent @event,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var signalRService = serviceProvider.GetRequiredService<ISignalRNotificationService>();
            var repository = serviceProvider.GetRequiredService<INotificationRepository>();

            var notification = await repository.GetByIdAsync(@event.NotificationId, cancellationToken);
            if (notification == null) return;

            var userId = notification.Metadata?.GetValueOrDefault("userId");
            if (string.IsNullOrEmpty(userId)) return;

            var dto = MapToDto(notification);
            await signalRService.SendToUserAsync(userId, dto, cancellationToken);
        }

        private async Task HandleNotificationSentAsync(
            NotificationSentEvent @event,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var signalRService = serviceProvider.GetRequiredService<ISignalRNotificationService>();
            var repository = serviceProvider.GetRequiredService<INotificationRepository>();

            var notification = await repository.GetByIdAsync(@event.NotificationId, cancellationToken);
            if (notification == null) return;

            var userId = notification.Metadata?.GetValueOrDefault("userId");
            if (!string.IsNullOrEmpty(userId))
            {
                await signalRService.SendStatusUpdateAsync(
                    @event.NotificationId,
                    NotificationStatus.Sent,
                    userId,
                    cancellationToken);
            }
        }

        private async Task HandleNotificationDeliveredAsync(
            NotificationDeliveredEvent @event,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var signalRService = serviceProvider.GetRequiredService<ISignalRNotificationService>();
            var repository = serviceProvider.GetRequiredService<INotificationRepository>();

            var notification = await repository.GetByIdAsync(@event.NotificationId, cancellationToken);
            if (notification == null) return;

            var userId = notification.Metadata?.GetValueOrDefault("userId");
            if (!string.IsNullOrEmpty(userId))
            {
                await signalRService.SendStatusUpdateAsync(
                    @event.NotificationId,
                    NotificationStatus.Delivered,
                    userId,
                    cancellationToken);
            }
        }

        private async Task HandleNotificationFailedAsync(
            NotificationFailedEvent @event,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var signalRService = serviceProvider.GetRequiredService<ISignalRNotificationService>();
            var repository = serviceProvider.GetRequiredService<INotificationRepository>();

            var notification = await repository.GetByIdAsync(@event.NotificationId, cancellationToken);
            if (notification == null) return;

            var userId = notification.Metadata?.GetValueOrDefault("userId");
            if (!string.IsNullOrEmpty(userId))
            {
                await signalRService.SendStatusUpdateAsync(
                    @event.NotificationId,
                    NotificationStatus.Failed,
                    userId,
                    cancellationToken);
            }
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto(
                notification.Id,
                notification.CorrelationId,
                notification.Channel,
                notification.Recipient.Name,
                notification.Content.Subject,
                notification.Content.Body,
                notification.Status,
                notification.Priority,
                notification.CreatedAt,
                notification.SentAt,
                notification.ErrorMessage,
                notification.Metadata);
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Service Bus error occurred");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }
    }
}
