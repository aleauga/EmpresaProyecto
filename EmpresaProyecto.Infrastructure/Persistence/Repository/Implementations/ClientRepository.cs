using EmpresaProyecto.Core.Entities;
using EmpresaProyecto.Core.Repository.Contracts;
using EmpresaProyecto.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EmpresaProyecto.Infrastructure.Persistence.Repository.Implementations
{
    public class ClientRepository : IClientRepository
    {
        private readonly SubscriptionContext _context;
        public ClientRepository(SubscriptionContext context)
        {
            _context = context;
        }

        public async Task<Cliente> CreateClient(Cliente cliente)
        {
            try
            {
                _context.Cliente.Add(cliente);
                await _context.SaveChangesAsync();
                return cliente;
            }
            catch (ValidationException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Cliente> GetClientByClientId(string clientId)
        {
            try
            {
                return await _context.Cliente.Where(x => x.IdCliente == clientId).FirstOrDefaultAsync();

            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
