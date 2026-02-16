using EmpresaProyecto.Core.Messaging.Contracts;
using EmpresaProyecto.Core.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EmpresaProyecto.Infrastructure.Messaging
{
    public class RabbitPublisher : IEventPublisher
    {
        private readonly RabbitSettings _rabbitSettings;
        private readonly IConnectionFactory _connectionFactory;

        public RabbitPublisher(IOptions<RabbitSettings> rabbitSettings, IConnectionFactory? connectionFactory = null)
        {
            _rabbitSettings = rabbitSettings.Value;
            _connectionFactory = connectionFactory ?? new ConnectionFactory()
            {
                HostName = _rabbitSettings.HostName,
                Port = _rabbitSettings.Port,
                UserName = _rabbitSettings.UserName,
                Password = _rabbitSettings.Password
            };
        }

        public async Task PublishAsync<TEvent>(TEvent message)
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(queue: _rabbitSettings.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var serializeMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(serializeMessage);
            var properties = new BasicProperties { Persistent = true };
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _rabbitSettings.QueueName,
                mandatory: false,
                basicProperties: properties,
                body: body);
        }
    }
}
