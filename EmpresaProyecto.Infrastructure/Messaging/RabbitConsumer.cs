using EmpresaProyecto.Core.Messaging.Contracts;
using EmpresaProyecto.Core.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EmpresaProyecto.Infrastructure.Messaging
{
    public class RabbitConsumer : IEventConsumer
    {
        private readonly RabbitSettings _settings;
        private IChannel? _channel;
        private IConnection? _connection;
        public RabbitConsumer(IOptions<RabbitSettings> settings)
        {
            _settings = settings.Value;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

        public async Task InitEventConsumerAsync(Func<string, Task> eventCallback)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            // Reintentos con exponential backoff
            int maxRetries = 10;
            int retryCount = 0;
            TimeSpan delay = TimeSpan.FromSeconds(2);

            while (retryCount < maxRetries)
            {
                try
                {
                    Console.WriteLine($"Intentando conectar a RabbitMQ ({_settings.HostName}:{_settings.Port})...");
                    _connection = await factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();
                    Console.WriteLine("Conexión a RabbitMQ exitosa");
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"Error al conectar a RabbitMQ (intento {retryCount}/{maxRetries}): {ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        throw new InvalidOperationException(
                            $"No se pudo conectar a RabbitMQ después de {maxRetries} intentos", ex);
                    }

                    await Task.Delay(delay);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 30)); // Max 30 segundos
                }
            }

            await _channel.QueueDeclareAsync(queue: _settings.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await eventCallback(message);
                // Procesamiento asíncrono aquí
                await Task.Yield();
            };

            await _channel.BasicConsumeAsync(queue: _settings.QueueName, autoAck: true, consumer: consumer);

        }
    }
}