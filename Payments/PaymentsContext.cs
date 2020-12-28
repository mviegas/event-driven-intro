using Microsoft.EntityFrameworkCore;

namespace Payments
{
    public class PaymentsContext : DbContext
    {
        public PaymentsContext()
        {
            
        }

        public PaymentsContext(DbContextOptions<PaymentsContext> options) : base(options)
        {
            
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Name=postgres");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("payments");
            
            base.OnModelCreating(modelBuilder);
        }
    }
}