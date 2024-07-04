using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace OnlineShop.EntityHistory.Console;

public class EntityHistoryDbContext : DbContext
{
    public DbSet<EntityChangedMessage> EntityChanges { get; set; }

    public EntityHistoryDbContext(DbContextOptions<EntityHistoryDbContext> options) : base(options)
    {
    }
}