using InventoryService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Persistence.Database
{
    public class InventoryDbContext : DbContext
    {
        public virtual DbSet<Product> Products { get; set; }
        
        public InventoryDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {}

        public static InventoryDbContext Create(string connectionString) =>
            new InventoryDbContext(GetSqlServerOptions(connectionString));

        private static DbContextOptions<InventoryDbContext> GetSqlServerOptions(string connectionString) =>
            new DbContextOptionsBuilder<InventoryDbContext>().UseSqlServer<InventoryDbContext>(connectionString).Options;
    }
}