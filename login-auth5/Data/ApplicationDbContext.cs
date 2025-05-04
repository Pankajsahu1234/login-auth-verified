using Microsoft.EntityFrameworkCore;
using login_auth5.Models;
using login_auth5.Models;

namespace login_auth5.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }

        // Optional: Customize the schema/model if needed
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // You can configure entity properties here
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique(false);

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Phone)
                .IsUnique(false);
        }
    }
}