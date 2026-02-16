using EmpresaProyecto.API.Subscriptions.DTO;
using EmpresaProyecto.API.Subscriptions.Services.Implementations;
using EmpresaProyecto.Core.Entities;
using EmpresaProyecto.Core.Messaging.Contracts;
using EmpresaProyecto.Core.Messaging.Events;
using EmpresaProyecto.Core.Repository.Contracts;
using EmpresaProyecto.Infrastructure.Persistence.Context;
using EmpresaProyecto.Tests.Utilities;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using System;

namespace EmpresaProyecto.Tests.Services
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepoMock;
        private readonly Mock<IClientRepository> _clientRepoMock;
        private readonly Mock<IValidator<Cliente>> _validatorMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly SubscriptionContext _context; // puedes usar un fake o mock si lo necesitas

        private readonly SubscriptionService _service;

        public SubscriptionServiceTests()
        {
            _subscriptionRepoMock = new Mock<ISubscriptionRepository>();
            _clientRepoMock = new Mock<IClientRepository>();
            _validatorMock = new Mock<IValidator<Cliente>>();
            _eventPublisherMock = new Mock<IEventPublisher>();

            // Create DbContextOptions for SubscriptionContext using InMemory provider and pass to constructor.
            var options = new DbContextOptionsBuilder<SubscriptionContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _context = new SubscriptionContext(options); // fix: provide required options

            _service = new SubscriptionService(
                _subscriptionRepoMock.Object,
                _clientRepoMock.Object,
                _context,
                _validatorMock.Object,
                _eventPublisherMock.Object
            );
        }

        [Fact]
        public async Task CreateSubscription_ThrowsValidationException_WhenClientIsInvalid()
        {
            // Arrange
            var dto = new SubscriptionRequestDTO { Nombre = "Alejandra" };
            var validationResult = new ValidationResult(new[] { new ValidationFailure("Nombre", "Error") });
            _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Cliente>(), default))
                          .ReturnsAsync(validationResult);

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _service.CreateSubscription(dto));
        }

        [Fact]
        public async Task CreateSubscription_CreatesClientAndSubscription_WhenValid()
        {
            // Arrange
            var context = DbContextFactory.CreateContext();

            var subscriptionRepoMock = new Mock<ISubscriptionRepository>();
            var clientRepoMock = new Mock<IClientRepository>();
            var validatorMock = new Mock<IValidator<Cliente>>();
            var eventPublisherMock = new Mock<IEventPublisher>();

            var dto = new SubscriptionRequestDTO
            {
                Nombre = "Alejandra",
                ApellidoPaterno = "García",
                Correo = "alejandra@example.com",
                Plan = "Basic",
                Tarjeta = new DatosPagoDto { NumeroTarjeta = "1", Expiracion = "1", Cvv = "1" }
            };

            var cliente = new Cliente { IdCliente = Guid.NewGuid().ToString(), Nombre = dto.Nombre, ApellidoPaterno = dto.ApellidoPaterno, ApellidoMaterno = dto.ApellidoMaterno, Correo = dto.Correo, Telefono = dto.Telefono };
            var suscripcion = new Suscripcion { IdSuscripcion = 1, IdCliente = cliente.IdCliente };

            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<Cliente>(), default))
                         .ReturnsAsync(new ValidationResult());

            clientRepoMock.Setup(c => c.CreateClient(It.IsAny<Cliente>()))
                          .ReturnsAsync(cliente);

            subscriptionRepoMock.Setup(s => s.CreateSubscription(It.IsAny<Suscripcion>()))
                                .Returns(Task.CompletedTask);

            eventPublisherMock.Setup(e => e.PublishAsync(It.IsAny<SubscriptionRequestedEvent>()))
                              .Returns(Task.CompletedTask);

            var service = new SubscriptionService(
                subscriptionRepoMock.Object,
                clientRepoMock.Object,
                context,
                validatorMock.Object,
                eventPublisherMock.Object
            );

            // Act
            await service.CreateSubscription(dto);

            // Assert
            clientRepoMock.Verify(c => c.CreateClient(It.IsAny<Cliente>()), Times.Once);
            subscriptionRepoMock.Verify(s => s.CreateSubscription(It.IsAny<Suscripcion>()), Times.Once);
            eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<SubscriptionRequestedEvent>()), Times.Once);
        }

    }
}
