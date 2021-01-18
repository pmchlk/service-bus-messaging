using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Models;

namespace PaymentService.Persistence.Database
{
    public class PaymentDbContext : DbContext
    {
        public virtual DbSet<Payment> Payments { get; set; }
        
        public PaymentDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {}

        public static PaymentDbContext Create(string connectionString) =>
            new PaymentDbContext(GetSqlServerOptions(connectionString));

        private static DbContextOptions<PaymentDbContext> GetSqlServerOptions(string connectionString) =>
            new DbContextOptionsBuilder<PaymentDbContext>().UseSqlServer<PaymentDbContext>(connectionString).Options;
    }
}