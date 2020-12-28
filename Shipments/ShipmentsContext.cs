using Microsoft.EntityFrameworkCore;

namespace Shipments
{
    public class ShipmentsContext : DbContext
    {
        public ShipmentsContext()
        {
            
        }

        public ShipmentsContext(DbContextOptions<ShipmentsContext> options) : base(options)
        {
            
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Name=postgres");
            }
        }
    }
}