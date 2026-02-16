using EmpresaProyecto.Core.Entities;
using EmpresaProyecto.Core.Repository.Contracts;
using EmpresaProyecto.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EmpresaProyecto.Infrastructure.Persistence.Repository.Implementations
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly SubscriptionContext _context;
        public SubscriptionRepository(SubscriptionContext context)
        {
            _context = context;
        }
        public async Task CreateSubscription(Suscripcion suscripcion)
        {
            try
            {
                _context.Suscripcion.Add(suscripcion);
                await _context.SaveChangesAsync();
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
        public async Task UpdateSubscription(Suscripcion suscripcion)
        {
            try
            {
                _context.Suscripcion.Update(suscripcion);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Suscripcion> GetSubscriptionBySubscriptionId(long subscriptionId)
        {
            try
            {
                var subscription = await _context.Suscripcion.Where(x => x.IdSuscripcion == subscriptionId).FirstOrDefaultAsync();
                return subscription;

            }
            catch (Exception)
            {
                throw;
            }

        }
        public async Task<Suscripcion> GetSubscriptionByClientId(string clientId)
        {
            try
            {
                var subscription = await _context.Suscripcion.Where(x => x.IdCliente == clientId).FirstOrDefaultAsync();
                return subscription;

            }
            catch (Exception)
            {
                throw;
            }

        }
        
    }
}
