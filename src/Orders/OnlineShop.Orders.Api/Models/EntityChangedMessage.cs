using OnlineShop.Orders.Api.Enums;

namespace OnlineShop.Orders.Api.Models
{
    public class EntityChangedMessage
    {
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public EntityChangeType ChangeType { get; set; }
        public string? NewValue { get; set; }
    }
}
