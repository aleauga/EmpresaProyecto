
using EmpresaProyecto.API.Subscriptions.DTO;

namespace EmpresaProyecto.API.Subscriptions.Services.Contracts
{
    public interface ISubscriptionService
    {
        Task CreateSubscription(SubscriptionRequestDTO requestDTO);
        Task<SubscriptionResponseDTO> GetCLientSubscription(string clientId);
    }
}
