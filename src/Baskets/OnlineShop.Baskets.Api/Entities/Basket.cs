using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Baskets.Api.Entities;

public class Basket
{
	public int Id { get; set; }

	public int UserId { get; set; }

	public List<int>? BasketProductIds { get; set; }
}
