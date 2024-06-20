using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Orders.Api.Models;

namespace OnlineShop.Orders.Api.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(item => item.Id);
            builder.Property(i => i.Price).HasColumnType("decimal(18,2)");
        }
    }
}
