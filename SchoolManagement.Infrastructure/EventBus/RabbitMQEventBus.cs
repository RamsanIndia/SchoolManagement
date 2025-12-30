//using Microsoft.Extensions.Logging;
//using RabbitMQ.Client;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace SchoolManagement.Infrastructure.EventBus
//{
//    public class RabbitMQEventBus : IEventBus
//    {
//        private readonly ILogger<RabbitMQEventBus> _logger;
//        private readonly IServiceProvider _serviceProvider;
//        private readonly IConnection _connection;
//        private readonly IModel _channel;

//        public RabbitMQEventBus(
//            ILogger<RabbitMQEventBus> logger,
//            IServiceProvider serviceProvider,
//            string hostName)
//        {
//            _logger = logger;
//            _serviceProvider = serviceProvider;

//            var factory = new ConnectionFactory
//            {
//                HostName = hostName
//            };

//            _connection = factory.CreateConnection();
//            _channel = _connection.CreateModel();
//        }

//        public Task PublishAsync<T>(T @event) where T : IEvent
//        {
//            var queueName = typeof(T).Name;

//            _channel.QueueDeclare(
//                queue: queueName,
//                durable: true,
//                exclusive: false,
//                autoDelete: false,
//                arguments: null
//            );

//            var body = JsonSerializer.SerializeToUtf8Bytes(@event);

//            _channel.BasicPublish(
//                exchange: "",
//                routingKey: queueName,
//                basicProperties: null,
//                body: body
//            );

//            _logger.LogInformation("RabbitMQ: Event published -> {EventType}", typeof(T).Name);

//            return Task.CompletedTask;
//        }

//        public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IEvent
//        {
//            throw new NotImplementedException();
//        }

//        public Task PublishBatchAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default) where T : IEvent
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
