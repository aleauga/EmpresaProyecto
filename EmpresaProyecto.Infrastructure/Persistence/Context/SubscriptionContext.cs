using EmpresaProyecto.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmpresaProyecto.Infrastructure.Persistence.Context
{
    public class SubscriptionContext : DbContext
    {

        public SubscriptionContext(DbContextOptions<SubscriptionContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>().Property(k => k.IdCliente).ValueGeneratedOnAdd();
            modelBuilder.Entity<Suscripcion>().HasKey(k => new { k.IdSuscripcion });
        }

        #region DbSet Members 
        public DbSet<Cliente> Cliente { get; set; }
        public DbSet<Suscripcion> Suscripcion { get; set; }
        #endregion
    }
}
