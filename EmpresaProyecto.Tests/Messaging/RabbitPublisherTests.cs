using EmpresaProyecto.Core.Models;
using EmpresaProyecto.Infrastructure.Messaging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EmpresaProyecto.Tests.Messaging
{
    public class RabbitPublisherTests
    {
        private readonly RabbitSettings _validSettings;

        public RabbitPublisherTests()
        {
            _validSettings = new RabbitSettings
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                QueueName = "test-queue"
            };
        }

        [Fact]
        public async Task PublishAsync_WithValidMessage_ShouldPublishSuccessfully()
        {
            // Arrange
            var mockChannel = new Mock<IChannel>();
            var mockConnection = new Mock<IConnection>();
            var mockFactory = new Mock<IConnectionFactory>();

            mockChannel
                .Setup(c => c.QueueDeclareAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<System.Collections.Generic.IDictionary<string, object?>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueueDeclareOk(_validSettings.QueueName, 0, 0));

            mockChannel
                .Setup(c => c.BasicPublishAsync<BasicProperties>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<BasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(ValueTask.CompletedTask);

            mockConnection
                .Setup(c => c.CreateChannelAsync(
                    It.IsAny<CreateChannelOptions?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockChannel.Object);

            mockConnection
                .Setup(c => c.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            mockChannel
                .Setup(c => c.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            mockFactory
                .Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockConnection.Object);

            var settings = Options.Create(_validSettings);
            var publisher = new RabbitPublisher(settings, mockFactory.Object);

            var testMessage = new { Id = 1, Text = "Test Message" };

            // Act
            await publisher.PublishAsync(testMessage);

            // Assert
            mockChannel.Verify(c => c.QueueDeclareAsync(
                _validSettings.QueueName,
                false,
                false,
                false,
                null,
                false,
                false,
                It.IsAny<CancellationToken>()), Times.Once);

            mockChannel.Verify(c => c.BasicPublishAsync<BasicProperties>(
                "",
                _validSettings.QueueName,
                false,
                It.IsAny<BasicProperties>(),
                It.IsAny<ReadOnlyMemory<byte>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PublishAsync_SerializesMessageCorrectly()
        {
            // Arrange
            var mockChannel = new Mock<IChannel>();
            var mockConnection = new Mock<IConnection>();
            var mockFactory = new Mock<IConnectionFactory>();

            mockChannel
                .Setup(c => c.QueueDeclareAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<System.Collections.Generic.IDictionary<string, object?>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueueDeclareOk(_validSettings.QueueName, 0, 0));

            ReadOnlyMemory<byte>? capturedBody = null;

            mockChannel
                .Setup(c => c.BasicPublishAsync<BasicProperties>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<BasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, string, bool, BasicProperties, ReadOnlyMemory<byte>, CancellationToken>(
                    (exchange, routingKey, mandatory, props, body, ct) =>
                    {
                        capturedBody = body;
                    })
                .Returns(ValueTask.CompletedTask);

            mockConnection
                .Setup(c => c.CreateChannelAsync(
                    It.IsAny<CreateChannelOptions?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockChannel.Object);

            mockConnection
                .Setup(c => c.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            mockChannel
                .Setup(c => c.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            mockFactory
                .Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockConnection.Object);

            var settings = Options.Create(_validSettings);
            var publisher = new RabbitPublisher(settings, mockFactory.Object);

            var testMessage = new { Id = 1, Text = "Test Message" };
            var expectedJson = JsonSerializer.Serialize(testMessage);

            // Act
            await publisher.PublishAsync(testMessage);

            // Assert
            Assert.NotNull(capturedBody);
            var actualJson = Encoding.UTF8.GetString(capturedBody.Value.Span);
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public async Task PublishAsync_SetsPersistentProperty()
        {
            // Arrange
            var mockChannel = new Mock<IChannel>();
            var mockConnection = new Mock<IConnection>();
            var mockFactory = new Mock<IConnectionFactory>();

            mockChannel
                .Setup(c => c.QueueDeclareAsync(
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<System.Collections.Generic.IDictionary<string, object?>>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new QueueDeclareOk(_validSettings.QueueName, 0, 0));

            BasicProperties? capturedProperties = null;

            mockChannel
                .Setup(c => c.BasicPublishAsync<BasicProperties>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<BasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, string, bool, BasicProperties, ReadOnlyMemory<byte>, CancellationToken>(
                    (exchange, routingKey, mandatory, props, body, ct) =>
                    {
                        capturedProperties = props;
                    })
                .Returns(ValueTask.CompletedTask);

            mockConnection
                .Setup(c => c.CreateChannelAsync(
                    It.IsAny<CreateChannelOptions?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockChannel.Object);

            mockConnection
                .Setup(c => c.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            mockChannel
                .Setup(c => c.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            mockFactory
                .Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockConnection.Object);

            var settings = Options.Create(_validSettings);
            var publisher = new RabbitPublisher(settings, mockFactory.Object);

            var testMessage = new { Id = 1 };

            // Act
            await publisher.PublishAsync(testMessage);

            // Assert
            Assert.NotNull(capturedProperties);
            Assert.True(capturedProperties.Persistent);
        }
    }
}
