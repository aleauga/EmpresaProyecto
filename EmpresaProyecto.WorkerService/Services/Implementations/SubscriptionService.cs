using EmpresaProyecto.Core.Enums;
using EmpresaProyecto.Core.Messaging.Events;
using EmpresaProyecto.Core.Repository.Contracts;
using EmpresaProyecto.Core.Rest.Contracts;
using EmpresaProyecto.Infrastructure.Communication;
using EmpresaProyecto.WorkerService.Services.Contracts;
using Microsoft.AspNetCore.SignalR;
using Polly;

namespace EmpresaProyecto.WorkerService.Services.Implementations
{
    public class SubscriptionService(ISubscriptionRepository _repository,IPaymentGateway _payment, ResiliencePipeline _pipeline, IHubContext<NotificationHub> _hub) : ISubscriptionService
    {
        public async Task SubscriptionRequestedHandler(SubscriptionRequestedEvent subscriptionRequested)
        {
            try
            {
                var subscription = await _repository.GetSubscriptionBySubscriptionId(subscriptionRequested.IdSuscripcion);
                if (subscription != null && subscription.Estado == SubscriptionStateEnum.Pending.ToString())
                {
                    bool pagoValido = await _pipeline.ExecuteAsync(async token =>
                    {
                        return await _payment.ValidatePaymentAsync();
                    }); //pagoFicticio// resiliencia aplicada automáticamente

                    if (pagoValido)
                    {
                        var today = DateTime.UtcNow;
                        subscription.FechaPago = today;
                        subscription.UltimaFechaModificacion = today;
                        subscription.Estado = SubscriptionStateEnum.Active.ToString();
                        await _repository.UpdateSubscription(subscription);

                        // Notificar solo al usuario dueño de la suscripción
                        await _hub.Clients.Group(subscription.IdCliente.ToString()).SendAsync("PagoCompletado",subscription.IdSuscripcion.ToString());


                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
