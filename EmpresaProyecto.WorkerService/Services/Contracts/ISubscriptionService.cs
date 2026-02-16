using EmpresaProyecto.Core.Messaging.Events;

namespace EmpresaProyecto.WorkerService.Services.Contracts
{
    public interface ISubscriptionService
    {
        Task SubscriptionRequestedHandler(SubscriptionRequestedEvent subscriptionRequested);
    }
}
