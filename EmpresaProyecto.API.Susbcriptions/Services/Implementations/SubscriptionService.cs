using EmpresaProyecto.API.Subscriptions.DTO;
using EmpresaProyecto.API.Subscriptions.Helpers;
using EmpresaProyecto.API.Subscriptions.Services.Contracts;
using EmpresaProyecto.Core.Entities;
using EmpresaProyecto.Core.Enums;
using EmpresaProyecto.Core.Messaging.Contracts;
using EmpresaProyecto.Core.Messaging.Events;
using EmpresaProyecto.Core.Repository.Contracts;
using EmpresaProyecto.Infrastructure.Persistence.Context;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EmpresaProyecto.API.Subscriptions.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        public readonly ISubscriptionRepository _subscription;
        public readonly IClientRepository _client;
        public readonly SubscriptionContext _context;
        private readonly IValidator<Cliente> _validator;
        private readonly IEventPublisher _eventPublisher;

        public SubscriptionService(ISubscriptionRepository subscription, IClientRepository client, SubscriptionContext context, IValidator<Cliente> validator, IEventPublisher eventPublisher)
        {
            _client = client;
            _subscription = subscription;
            _context = context;
            _validator = validator;
            _eventPublisher = eventPublisher;

        }
        public async Task CreateSubscription(SubscriptionRequestDTO requestDTO)
        {
            try
            {
                var suscripcion = new Suscripcion();
                var client = Mappers.ConverTo(requestDTO);
                var result = await _validator.ValidateAsync(client);

                if (!result.IsValid)
                {
                    throw new FluentValidation.ValidationException(result.Errors);
                }
                var transactionService = new TransactionService(_context);

                await transactionService.ExecuteInTransactionAsync(async () =>
                {
                    client = await _client.CreateClient(client);

                    var dateNow = DateTime.UtcNow;
                    suscripcion = new Suscripcion
                    {
                        IdCliente = client.IdCliente,
                        FechaCreacion = dateNow,
                        UltimaFechaModificacion = dateNow,
                        Estado = SubscriptionStateEnum.Pending.ToString(),
                        Plan = requestDTO.Plan
                    };

                    await _subscription.CreateSubscription(suscripcion);
                });

                var requestedEvent = new SubscriptionRequestedEvent
                {
                    IdCliente = client.IdCliente,
                    IdSuscripcion = suscripcion.IdSuscripcion,
                    MetodoPagoEncriptado = requestDTO.Tarjeta.ToString()
                };

                await _eventPublisher.PublishAsync(requestedEvent);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SubscriptionResponseDTO> GetCLientSubscription(string clientId)
        {
            try
            {
                var client = await _client.GetClientByClientId(clientId);
                var subscription = await _subscription.GetSubscriptionByClientId(clientId);
                return Mappers.ConverTo(client, subscription);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
