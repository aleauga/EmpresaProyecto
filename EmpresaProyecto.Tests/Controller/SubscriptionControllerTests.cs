using EmpresaProyecto.API.Subscriptions.Controllers;
using EmpresaProyecto.API.Subscriptions.DTO;
using EmpresaProyecto.API.Subscriptions.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace EmpresaProyecto.Tests.Controller
{
    public class SubscriptionControllerTests
    {
        private readonly Mock<ISubscriptionService> _serviceMock;
        private readonly SubscriptionController _controller;

        public SubscriptionControllerTests()
        {
            _serviceMock = new Mock<ISubscriptionService>();
            _controller = new SubscriptionController(_serviceMock.Object);
        }

        [Fact]
        public async Task CreateSubscription_ReturnsBadRequest_WhenRequestIsNull()
        {
            // Act
            var result = await _controller.CreateSubscription((SubscriptionRequestDTO)null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Datos inválidos", badRequest.Value);
        }

        [Fact]
        public async Task CreateSubscription_ReturnsAccepted_WhenRequestIsValid()
        {
            // Arrange
            var request = new SubscriptionRequestDTO
            {
                Nombre = "Alejandra",
                Correo = "test@example.com"
            };

            _serviceMock.Setup(s => s.CreateSubscription(request))
                        .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateSubscription(request);

            // Assert
            Assert.IsType<AcceptedResult>(result);
            _serviceMock.Verify(s => s.CreateSubscription(request), Times.Once);
        }

        [Fact]
        public async Task CreateSubscription_ThrowsException_WhenServiceFails()
        {
            // Arrange
            var request = new SubscriptionRequestDTO { Nombre = "Alejandra" };

            _serviceMock.Setup(s => s.CreateSubscription(request))
                        .ThrowsAsync(new Exception("Error interno"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateSubscription(request));
        }
        [Fact]
        public async Task GetClientSubscription_ShouldReturnOk_WhenServiceReturnsData()
        {
            // Arrange
            var clientId = "cliente-123";
            var expected = new SubscriptionResponseDTO
            {
                IdCliente = clientId,
                Plan = "Premium",
                Estado = "Activo"
            };

            var serviceMock = new Mock<ISubscriptionService>();
            serviceMock.Setup(s => s.GetCLientSubscription(clientId))
                       .ReturnsAsync(expected);

            var controller = new SubscriptionController(serviceMock.Object);

            // Act
            var result = await controller.GetCLientSubscription(clientId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task GetClientSubscription_ShouldThrowException_WhenServiceFails()
        {
            // Arrange
            var clientId = "cliente-999";
            var serviceMock = new Mock<ISubscriptionService>();
            serviceMock.Setup(s => s.GetCLientSubscription(clientId))
                       .ThrowsAsync(new InvalidOperationException("Error simulado"));

            var controller = new SubscriptionController(serviceMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await controller.GetCLientSubscription(clientId);
            });
        }


    }
}
