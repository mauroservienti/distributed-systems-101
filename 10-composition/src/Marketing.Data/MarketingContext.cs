using Marketing.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Marketing.Data
{
    public class MarketingContext : DbContext
    {
        public MarketingContext() { }

        public DbSet<ProductDetails> ProductsDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(@"Host=localhost;Port=5432;Username=db_user;Password=P@ssw0rd;Database=marketing_database");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductDetails>()
                .HasData(Initial.Data());

            base.OnModelCreating(modelBuilder);
        }

        internal static class Initial
        {
            internal static ProductDetails[] Data()
            {
                return new[]
                {
                    new ProductDetails()
                    {
                        Id = 1,
                        Name = "Banana Holder",
                        Description = "Outdoor travel cute banana protector storage box"
                    },
                    new ProductDetails()
                    {
                        Id = 2,
                        Name = "Nokia Lumia 635",
                        Description = "Amazing phone, unfortunately not understood by market"
                    }
                };
            }

        }
    }
}
