using EmpresaProyecto.Core.Entities;
using EmpresaProyecto.Infrastructure.Persistence.Context;
using EmpresaProyecto.Infrastructure.Persistence.Repository.Implementations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EmpresaProyecto.Tests.Repository
{
    public class SubscriptionRepositoryTests
    {
        private SubscriptionContext CreateContext()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<SubscriptionContext>()
                .UseSqlite(connection)
                .Options;

            var context = new SubscriptionContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task CreateSubscription_AddsSubscriptionToDatabase()
        {
            // Arrange
            var context = CreateContext();
            var repo = new SubscriptionRepository(context);

            var suscripcion = new Suscripcion
            {
                IdSuscripcion = 1,
                IdCliente = "cliente-123",
                Estado = "Pending",
                Plan = "Basic",
                FechaCreacion = DateTime.UtcNow,
                UltimaFechaModificacion = DateTime.UtcNow
            };

            // Act
            await repo.CreateSubscription(suscripcion);

            // Assert
            var saved = await context.Suscripcion.FirstOrDefaultAsync(s => s.IdSuscripcion == 1);
            Assert.NotNull(saved);
            Assert.Equal("cliente-123", saved.IdCliente);
            Assert.Equal("Basic", saved.Plan);
        }

        [Fact]
        public async Task UpdateSubscription_UpdatesExistingSubscription()
        {
            // Arrange
            var context = CreateContext();
            var repo = new SubscriptionRepository(context);

            var suscripcion = new Suscripcion
            {
                IdSuscripcion = 2,
                IdCliente = "cliente-456",
                Estado = "Pending",
                Plan = "Basic",
                FechaCreacion = DateTime.UtcNow,
                UltimaFechaModificacion = DateTime.UtcNow
            };

            context.Suscripcion.Add(suscripcion);
            await context.SaveChangesAsync();

            // Act
            suscripcion.Estado = "Active";
            await repo.UpdateSubscription(suscripcion);

            // Assert
            var updated = await context.Suscripcion.FirstOrDefaultAsync(s => s.IdSuscripcion == 2);
            Assert.NotNull(updated);
            Assert.Equal("Active", updated.Estado);
        }

        [Fact]
        public async Task GetSubscription_ReturnsSubscription_WhenExists()
        {
            // Arrange
            var context = CreateContext();
            var repo = new SubscriptionRepository(context);

            var suscripcion = new Suscripcion
            {
                IdSuscripcion = 3,
                IdCliente = "cliente-789",
                Estado = "Pending",
                Plan = "Premium",
                FechaCreacion = DateTime.UtcNow,
                UltimaFechaModificacion = DateTime.UtcNow
            };

            context.Suscripcion.Add(suscripcion);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetSubscriptionBySubscriptionId(3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("cliente-789", result.IdCliente);
            Assert.Equal("Premium", result.Plan);
        }

        [Fact]
        public async Task GetSubscription_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var context = CreateContext();
            var repo = new SubscriptionRepository(context);

            // Act
            var result = await repo.GetSubscriptionBySubscriptionId(999);

            // Assert
            Assert.Null(result);
        }

    }
}
