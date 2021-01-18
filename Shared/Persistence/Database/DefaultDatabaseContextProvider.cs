using Microsoft.EntityFrameworkCore;
using Shared.Persistence.Database.Meta;

namespace Shared.Persistence.Database
{
    public class DefaultDatabaseContextProvider<TContext> : IDatabaseContextProvider<TContext> where TContext : DbContext
    {
        private TContext _context;
        
        public DefaultDatabaseContextProvider(TContext context)
        {
            _context = context;
        }

        public TContext GetContext() => _context;

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}