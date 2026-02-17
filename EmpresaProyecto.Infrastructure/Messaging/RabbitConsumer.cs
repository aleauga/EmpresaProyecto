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

        // Inicializa el consumidor de eventos y ejecuta un callback cuando llega un mensaje
        public async Task InitEventConsumerAsync(Func<string, Task> eventCallback)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            // Reintentos con backoff exponencial
            int maxRetries = 10;                   
            int retryCount = 0;                    
            TimeSpan delay = TimeSpan.FromSeconds(2); // Delay inicial

            while (retryCount < maxRetries)
            {
                try
                {
                    Console.WriteLine($"Intentando conectar a RabbitMQ ({_settings.HostName}:{_settings.Port})...");
                    _connection = await factory.CreateConnectionAsync(); // Abre conexión TCP
                    _channel = await _connection.CreateChannelAsync();   // Crea canal de comunicación
                    Console.WriteLine("Conexión a RabbitMQ exitosa");
                    break; // Sale del bucle si la conexión fue exitosa
                }
                catch (Exception ex)
                {
                    retryCount++; // Incrementa contador de intentos
                    Console.WriteLine($"Error al conectar a RabbitMQ (intento {retryCount}/{maxRetries}): {ex.Message}");

                    if (retryCount >= maxRetries)
                    {
                        // Si se alcanzó el máximo de intentos, lanza excepción
                        throw new InvalidOperationException(
                            $"No se pudo conectar a RabbitMQ después de {maxRetries} intentos", ex);
                    }

                    await Task.Delay(delay); // Espera antes de reintentar
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 30)); // Incrementa delay hasta 30s
                }
            }

            // Declara la cola donde se consumirán mensajes
            await _channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: false,   // No persiste en disco
                exclusive: false, // Puede ser usada por múltiples consumidores
                autoDelete: false,// No se elimina automáticamente
                arguments: null);

            // Crea consumidor asíncrono
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();                   
                var message = Encoding.UTF8.GetString(body);    
                await eventCallback(message);                   // Ejecuta callback definido por el consumidor
                await Task.Yield();                             // Cede control para procesamiento asíncrono
            };

            // Inicia consumo de mensajes en la cola
            await _channel.BasicConsumeAsync(
                queue: _settings.QueueName,
                autoAck: true, // Marca mensajes como procesados automáticamente
                consumer: consumer);
        }
    }
}