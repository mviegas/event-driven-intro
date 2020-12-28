using Microsoft.EntityFrameworkCore;
using Orders.Domain;

namespace Orders.Infra
{
    public class OrdersContext : DbContext
    {
        public OrdersContext()
        {
            
        }

        public OrdersContext(DbContextOptions<OrdersContext> options) : base(options)
        {
            
        }
        
        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            if (!builder.IsConfigured)
            {
                builder.UseNpgsql("Name=postgres");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("orders");
            
            base.OnModelCreating(modelBuilder);
        }
    }
}