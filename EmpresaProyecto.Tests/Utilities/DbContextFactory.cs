
using EmpresaProyecto.Infrastructure.Persistence.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EmpresaProyecto.Tests.Utilities
{
    public static class DbContextFactory
    {
        public static SubscriptionContext CreateContext()
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
    }
}


