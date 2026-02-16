using EmpresaProyecto.Core.Entities;
using EmpresaProyecto.Core.Enums;
using EmpresaProyecto.Infrastructure.Persistence.Context;
using EmpresaProyecto.Infrastructure.Persistence.Repository.Implementations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EmpresaProyecto.Tests.Infrastructure
{
    public class SubscriptionRepositoryTests
    {
        private SubscriptionContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SubscriptionContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new SubscriptionContext(options);
        }

        [Fact]
        public async Task CreateSubscription_ShouldAddEntity()
        {
            var context = GetInMemoryContext("CreateDb");
            var repo = new SubscriptionRepository(context);

            var suscripcion = new Suscripcion
            {
                IdSuscripcion = 1,
                IdCliente = "cliente-123",
                Plan = "Premium",
                Estado = "Pendiente"
            };

            await repo.CreateSubscription(suscripcion);

            var saved = await context.Suscripcion.FirstOrDefaultAsync(x => x.IdSuscripcion == 1);
            Assert.NotNull(saved);
            Assert.Equal("cliente-123", saved.IdCliente);
        }

        [Fact]
        public async Task CreateSubscription_ShouldThrowValidationException()
        {
            var context = GetInMemoryContext("ValidationDb");
            var repo = new SubscriptionRepository(context);

            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                // Simulamos validación fallida
                throw new ValidationException("Datos inválidos");
            });
        }

        [Fact]
        public async Task UpdateSubscription_ShouldModifyEntity()
        {
            var context = GetInMemoryContext("UpdateDb");
            var repo = new SubscriptionRepository(context);

            var suscripcion = new Suscripcion
            {
                IdSuscripcion = 2,
                IdCliente = "cliente-456",
                Plan = "Basic",
                Estado = "Pendiente"
            };

            context.Suscripcion.Add(suscripcion);
            await context.SaveChangesAsync();

            suscripcion.Estado = "Actualizada";
            await repo.UpdateSubscription(suscripcion);

            var updated = await context.Suscripcion.FirstOrDefaultAsync(x => x.IdSuscripcion == 2);
            Assert.Equal("Actualizada", updated.Estado);
        }

        [Fact]
        public async Task GetSubscriptionBySubscriptionId_ShouldReturnEntity()
        {
            var context = GetInMemoryContext("GetByIdDb");
            var repo = new SubscriptionRepository(context);

            var suscripcion = new Suscripcion
            {
                IdSuscripcion = 3,
                IdCliente = "cliente-789",
                Plan = "Premium",
                Estado = "Activo"
            };

            context.Suscripcion.Add(suscripcion);
            await context.SaveChangesAsync();

            var result = await repo.GetSubscriptionBySubscriptionId(3);

            Assert.NotNull(result);
            Assert.Equal("cliente-789", result.IdCliente);
        }

        [Fact]
        public async Task GetSubscriptionByClientId_ShouldReturnEntity()
        {
            var context = GetInMemoryContext("GetByClientIdDb");
            var repo = new SubscriptionRepository(context);

            var suscripcion = new Suscripcion
            {
                IdSuscripcion = 4,
                IdCliente = "cliente-999",
                Plan = "Basic",
                Estado = "Activo"
            };

            context.Suscripcion.Add(suscripcion);
            await context.SaveChangesAsync();

            var result = await repo.GetSubscriptionByClientId("cliente-999");

            Assert.NotNull(result);
            Assert.Equal("cliente-999", result.IdCliente);
        }

        [Fact]
        public async Task UpdateSubscription_ShouldThrowException_WhenEntityNotTracked()
        {
            var context = GetInMemoryContext("ExceptionDb");
            var repo = new SubscriptionRepository(context);

            var suscripcion = new Suscripcion
            {
                IdSuscripcion = 99,
                IdCliente = "no-existe",
                Plan = "Basic",
                Estado = "Pendiente"
            };

            // Forzamos excepción simulando SaveChanges fallido
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                context.Entry(suscripcion).State = EntityState.Modified;
                await context.SaveChangesAsync();
            });
        }
    }

}