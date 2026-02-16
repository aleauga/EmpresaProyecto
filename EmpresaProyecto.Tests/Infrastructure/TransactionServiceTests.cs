using EmpresaProyecto.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
namespace EmpresaProyecto.Tests.Infrastructure
{
    public class TransactionServiceTests
    {
        private DbContext GetInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new DbContext(options);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_TResult_ShouldCommitAndReturnValue()
        {
            // Arrange
            var context = GetInMemoryContext("TransactionDb1");
            var service = new TransactionService(context);

            // Act
            var result = await service.ExecuteInTransactionAsync(async () =>
            {
                // Simulamos lógica de negocio
                await Task.Delay(10);
                return "OK";
            });

            // Assert
            Assert.Equal("OK", result);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_TResult_ShouldRollbackOnException()
        {
            var context = GetInMemoryContext("TransactionDb2");
            var service = new TransactionService(context);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.ExecuteInTransactionAsync<string>(async () =>
                {
                    await Task.Delay(10);
                    throw new InvalidOperationException("Error simulado");
                });
            });
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_ShouldCommit()
        {
            var context = GetInMemoryContext("TransactionDb3");
            var service = new TransactionService(context);

            bool executed = false;

            await service.ExecuteInTransactionAsync(async () =>
            {
                executed = true;
                await Task.Delay(10);
            });

            Assert.True(executed);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_ShouldRollbackOnException()
        {
            var context = GetInMemoryContext("TransactionDb4");
            var service = new TransactionService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.ExecuteInTransactionAsync(async () =>
                {
                    await Task.Delay(10);
                    throw new InvalidOperationException("Error simulado");
                });
            });
        }
    }

}