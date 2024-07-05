using Microsoft.EntityFrameworkCore;

namespace OnlineShop.EntityHistory.Console;

[Keyless]
public class EntityChangedMessage
{
    public string EntityName { get; set; }
    public int EntityId { get; set; }
    public EntityChangeType ChangeType { get; set; }
    public string? NewValue { get; set; }
}