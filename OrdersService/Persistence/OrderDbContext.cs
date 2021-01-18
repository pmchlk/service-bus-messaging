using Microsoft.EntityFrameworkCore;
using OrdersService.Domain.Models;

namespace OrdersService.Persistence
{
    public class OrderDbContext : DbContext
    {
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderLine> OrderLines { get; set; }
        
        public OrderDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {}

        public static OrderDbContext Create(string connectionString) =>
            new OrderDbContext(GetSqlServerOptions(connectionString));

        private static DbContextOptions<OrderDbContext> GetSqlServerOptions(string connectionString) =>
            new DbContextOptionsBuilder<OrderDbContext>().UseSqlServer<OrderDbContext>(connectionString).Options;
    }
}