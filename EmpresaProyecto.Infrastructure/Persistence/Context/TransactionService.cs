using Microsoft.EntityFrameworkCore;

namespace EmpresaProyecto.Infrastructure.Persistence.Context
{
    public class TransactionService
    {
        private readonly DbContext _context;

        public TransactionService(DbContext context)
        {
            _context = context;
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            // test
            if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                var result = await action();
                await _context.SaveChangesAsync();
                return result;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var result = await action();

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            // test
            if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                await action();
                await _context.SaveChangesAsync();
                return;
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await action();

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }

}
