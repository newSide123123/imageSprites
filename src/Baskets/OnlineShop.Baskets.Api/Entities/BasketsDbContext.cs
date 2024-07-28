using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace OnlineShop.Baskets.Api.Entities;

public class BasketsDbContext : DbContext
{
	public BasketsDbContext(DbContextOptions<BasketsDbContext> opts)
		: base(opts)
	{
	}

	public DbSet<Basket> Baskets { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		base.OnModelCreating(modelBuilder);
	}
}
