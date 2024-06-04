using Microsoft.EntityFrameworkCore;
using System.Reflection;
using OnlineShop.Store.Api.Models;

namespace OnlineShop.Store.Api.EF
{
    public class StoreDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public StoreDbContext()
        {

        }

        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
