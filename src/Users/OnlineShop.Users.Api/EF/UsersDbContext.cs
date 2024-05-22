using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Users.Api.Models;

namespace OnlineShop.Users.Api.EF;

public class UsersDbContext : DbContext
{
	public DbSet<User> Users { get; set; }

	public UsersDbContext()
	{
	}

	public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		base.OnModelCreating(modelBuilder);
	}
}