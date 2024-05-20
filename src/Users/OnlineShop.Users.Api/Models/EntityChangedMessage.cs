using OnlineShop.Users.Api.Enums;

namespace OnlineShop.Users.Api.Models;

public class EntityChangedMessage
{
    public string EntityName { get; set; }
    public int EntityId { get; set; }
    public EntityChangeType ChangeType { get; set; }
    public string? NewValue { get; set; }
}