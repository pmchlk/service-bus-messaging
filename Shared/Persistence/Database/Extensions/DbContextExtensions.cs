using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Persistence.Database.Meta;

namespace Shared.Persistence.Database.Extensions
{
    public static class DbContextExtensions
    {
        public static void AddSqlServerDbContext<TContext>(this IServiceCollection services, IConfiguration configuration,
            string connectionStringConfigurationKey, int commandTimeout = 5) where TContext : DbContext
        {
            var connectionString = configuration.GetValue<string>(connectionStringConfigurationKey);
            AddSqlServerDbContext<TContext>(services, connectionString, commandTimeout);
        }

        public static void AddSqlServerDbContext<TContext>(this IServiceCollection services, string connectionString,
            int commandTimeout = 5) where TContext : DbContext
        {
            services.AddDbContext<TContext>(options => options.UseSqlServer(connectionString, options =>
            {
                options.CommandTimeout((int) TimeSpan.FromMinutes(commandTimeout).TotalSeconds);
            }));
            services.AddScoped<IDatabaseContextProvider<TContext>, DefaultDatabaseContextProvider<TContext>>();

        }
    }
}