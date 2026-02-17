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

            // Si no se pasa una fábrica, se crea una nueva con los valores de configuración
            _connectionFactory = connectionFactory ?? new ConnectionFactory()
            {
                HostName = _rabbitSettings.HostName,
                Port = _rabbitSettings.Port,
                UserName = _rabbitSettings.UserName,
                Password = _rabbitSettings.Password
            };
        }

        // Método genérico para publicar un evento en RabbitMQ
        public async Task PublishAsync<TEvent>(TEvent message)
        {
            // Abre conexión y canal de manera asíncrona (se liberan automáticamente con await using)
            await using var connection = await _connectionFactory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // Declara la cola donde se enviará el mensaje
            await channel.QueueDeclareAsync(
                queue: _rabbitSettings.QueueName,
                durable: false,   // No persiste en disco
                exclusive: false, // Puede ser usada por múltiples consumidores
                autoDelete: false,// No se elimina automáticamente
                arguments: null);

            var serializeMessage = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(serializeMessage);

            // Propiedades del mensaje: Persistent=true indica que RabbitMQ debe intentar persistirlo
            var properties = new BasicProperties { Persistent = true };

            // Publica el mensaje en la cola
            await channel.BasicPublishAsync(
                exchange: "",                         // Exchange por defecto
                routingKey: _rabbitSettings.QueueName,// Nombre de la cola como routing key
                mandatory: false,                     // No requiere confirmación de entrega
                basicProperties: properties,          // Propiedades del mensaje
                body: body);                          // Contenido del mensaje en bytes
        }
    }
}