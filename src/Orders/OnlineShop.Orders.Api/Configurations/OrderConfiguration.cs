using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Orders.Api.Models;

namespace OnlineShop.Orders.Api.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(p => p.Id);
            builder.HasMany(p => p.Items)
                .WithOne(u => u.Order)
                .HasForeignKey(item => item.OrderId);
        }
    }
}
