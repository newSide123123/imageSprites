using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OnlineShop.EntityHistory.Console;

public class EntityHistoryContextFactory : IDesignTimeDbContextFactory<EntityHistoryDbContext>
{
    public EntityHistoryDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EntityHistoryDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5433;Database=entityHistory;Username=onlineshop;Password=yhivJp9hroOoY70pASHC")
            .Options;

        return new EntityHistoryDbContext(options);
    }
}