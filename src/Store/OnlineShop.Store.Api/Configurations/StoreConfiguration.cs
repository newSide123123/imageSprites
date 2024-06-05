using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineShop.Store.Api.Models;

namespace OnlineShop.Store.Api.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(b => b.Id);

        var products = new List<Product>()
        {
            new Product
            {
                Id = 1,
                Code = "j34k9",
                Name = "Samsung S32A600N",
                Description = "PC Monitor",
                Price = 12999.99,
            },
            new Product
            {
                Id = 2,
                Code = "83kl1",
                Name = "Akko 5075b Plus",
                Description = "Keyboard",
                Price = 4700
            },
            new Product
            {
                Id= 3,
                Code = "pw301",
                Name = "Apple Watch Ultra GPS",
                Description = "Hand watch",
                Price = 39999.99
            }
        };

        builder.HasData(products);
    }
}
