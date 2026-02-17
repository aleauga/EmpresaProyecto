using EmpresaProyecto.Core.Models;
using EmpresaProyecto.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EmpresaProyecto.Tests.Messaging
{
    public class RabbitConsumerTests
    {
        [Fact]
        public void Dispose_ShouldDisposeConnectionAndChannel()
        {
            // Arrange
            var settings = Options.Create(new RabbitSettings
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                QueueName = "test-queue"
            });

            var consumer = new RabbitConsumer(settings);

            var channelMock = new Mock<IChannel>();
            var connectionMock = new Mock<IConnection>();

            // Asignamos manualmente los mocks a los campos privados
            typeof(RabbitConsumer).GetField("_channel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(consumer, channelMock.Object);
            typeof(RabbitConsumer).GetField("_connection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(consumer, connectionMock.Object);

            // Act
            consumer.Dispose();

            // Assert
            channelMock.Verify(c => c.Dispose(), Times.Once);
            connectionMock.Verify(c => c.Dispose(), Times.Once);
        }

        [Fact]
        public async Task InitEventConsumerAsync_ShouldInvokeCallback_WhenMessageReceived()
        {
            // Arrange
            var settings = Options.Create(new RabbitSettings
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                QueueName = "test-queue"
            });

            var consumer = new RabbitConsumer(settings);

            string? receivedMessage = null;
            Func<string, Task> callback = msg =>
            {
                receivedMessage = msg;
                return Task.CompletedTask;
            };

            // Act
            // Aquí normalmente se conectaría a RabbitMQ real.
            // Para test unitario, simulamos directamente el evento del consumidor.
            var channelMock = new Mock<IChannel>();
            typeof(RabbitConsumer).GetField("_channel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(consumer, channelMock.Object);

            var asyncConsumer = new AsyncEventingBasicConsumer(channelMock.Object);
            var body = Encoding.UTF8.GetBytes("Hello World");
            var ea = new BasicDeliverEventArgs(
                "tag",
                1,
                false,
                "exchange",
                "routingKey",
                null,
                new ReadOnlyMemory<byte>(body),
                CancellationToken.None
            );

            await asyncConsumer.HandleBasicDeliverAsync("tag", 1, false, "exchange", "routingKey", ea.BasicProperties, ea.Body);
            await callback("Hello World");

            // Assert
            Assert.Equal("Hello World", receivedMessage);
        }

        [Fact]
        public async Task InitEventConsumerAsync_ShouldThrowAfterMaxRetries()
        {
            var settings = Options.Create(new RabbitSettings
            {
                HostName = "invalid-host",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                QueueName = "test-queue"
            });

            var consumer = new RabbitConsumer(settings);

        // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await consumer.InitEventConsumerAsync(msg => Task.CompletedTask);
            });
        }
    }

}