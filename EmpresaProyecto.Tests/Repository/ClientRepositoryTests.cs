
using EmpresaProyecto.Core.Entities;
using EmpresaProyecto.Infrastructure.Persistence.Context;
using EmpresaProyecto.Infrastructure.Persistence.Repository.Implementations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EmpresaProyecto.Tests.Repository
{
    public class ClientRepositoryTests
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
        public async Task CreateClient_AddsClientToDatabase()
        {
            // Arrange
            var context = CreateContext();
            var repo = new ClientRepository(context);

            var cliente = new Cliente
            {
                IdCliente = Guid.NewGuid().ToString(),
                Nombre = "Alejandra",
                ApellidoPaterno = "García",
                ApellidoMaterno = "López",
                Correo = "alejandra@example.com",
                Telefono = "555231234"
            };

            // Act
            var result = await repo.CreateClient(cliente);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cliente.IdCliente, result.IdCliente);

            var saved = await context.Cliente.FirstOrDefaultAsync(c => c.IdCliente == cliente.IdCliente);
            Assert.NotNull(saved);
            Assert.Equal("Alejandra", saved.Nombre);
        }

        [Fact]
        public async Task GetClient_ReturnsClient_WhenExists()
        {
            // Arrange
            var context = CreateContext();
            var repo = new ClientRepository(context);

            var cliente = new Cliente
            {
                IdCliente = Guid.NewGuid().ToString(),
                Nombre = "Alejandra",
                ApellidoPaterno = "García",
                ApellidoMaterno = "López",
                Correo = "alejandra@example.com",
                Telefono = "555231234"
            };

            context.Cliente.Add(cliente);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetClientByClientId(cliente.IdCliente);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(cliente.IdCliente, result.IdCliente);
            Assert.Equal("Alejandra", result.Nombre);
        }

        [Fact]
        public async Task GetClient_ReturnsNull_WhenNotExists()
        {
            // Arrange
            var context = CreateContext();
            var repo = new ClientRepository(context);

            // Act
            var result = await repo.GetClientByClientId("no-existe");

            // Assert
            Assert.Null(result);
        }

    }
}
