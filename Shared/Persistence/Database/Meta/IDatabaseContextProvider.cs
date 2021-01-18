using System;
using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence.Database.Meta
{
    public interface IDatabaseContextProvider<TContext> : IDisposable where TContext : DbContext
    {
        TContext GetContext();
    }
}