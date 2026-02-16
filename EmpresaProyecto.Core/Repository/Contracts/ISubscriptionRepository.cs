
using EmpresaProyecto.Core.Entities;

namespace EmpresaProyecto.Core.Repository.Contracts
{
    public interface ISubscriptionRepository
    {
        Task<Suscripcion> GetSubscriptionBySubscriptionId(long subscriptionId);
        Task<Suscripcion> GetSubscriptionByClientId(string clientId);
        Task CreateSubscription(Suscripcion suscripcion);
        Task UpdateSubscription(Suscripcion suscripcion);
    }
}
