

using EmpresaProyecto.Core.Entities;

namespace EmpresaProyecto.Core.Repository.Contracts
{
    public interface IClientRepository
    {
        Task<Cliente> CreateClient(Cliente cliente);
        Task<Cliente> GetClientByClientId(string clientId);
    }
}
