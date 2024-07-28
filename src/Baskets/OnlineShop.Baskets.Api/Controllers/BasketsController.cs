using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.ContentModel;
using OnlineShop.Baskets.Api.Entities;
using OnlineShop.Baskets.Api.Enums;
using OnlineShop.Baskets.Api.Models;
using OnlineShop.Baskets.Api.Options;
using RabbitMQ.Client;

namespace OnlineShop.Baskets.Api.Controllers;

[Route("api/baskets")]
[ApiController]
public class BasketsController : ControllerBase
{
	private readonly BasketsDbContext _context;
	private readonly HttpClient _httpClient;
	private readonly IConfiguration _configuration;
	private readonly RabbitMQOptions _rabbitMqOptions;
	private readonly ServiceUrls _serviceUrls;

	private readonly ILogger<BasketsController> _logger;
	private readonly IModel _channel;

	public BasketsController(BasketsDbContext context, HttpClient httpClient, IConfiguration configuration,
		IOptions<ServiceUrls> serviceUrls, IOptions<RabbitMQOptions> rabbitMqOptions, ILogger<BasketsController> logger)
	{
		_context = context;
		_httpClient = httpClient;
		_rabbitMqOptions = rabbitMqOptions.Value;
		_serviceUrls = serviceUrls.Value;

		var connectionFactory = new ConnectionFactory
		{
			HostName = _rabbitMqOptions.Host,
			Port = _rabbitMqOptions.Port,
			UserName = _rabbitMqOptions.Username,
			Password = _rabbitMqOptions.Password
		};
		var connection = connectionFactory.CreateConnection();
		var channel = connection.CreateModel();

		channel.ExchangeDeclare(_rabbitMqOptions.EntityExchange, "direct", false, false, null);

		channel.QueueDeclare(_rabbitMqOptions.EntityCreateQueue, false, false, false, null);
		channel.QueueBind(_rabbitMqOptions.EntityCreateQueue, _rabbitMqOptions.EntityExchange, "create");

		channel.QueueDeclare(_rabbitMqOptions.EntityUpdateQueue, false, false, false, null);
		channel.QueueBind(_rabbitMqOptions.EntityUpdateQueue, _rabbitMqOptions.EntityExchange, "update");

		channel.QueueDeclare(_rabbitMqOptions.EntityDeleteQueue, false, false, false, null);
		channel.QueueBind(_rabbitMqOptions.EntityDeleteQueue, _rabbitMqOptions.EntityExchange, "delete");

		_channel = channel;
		_logger = logger;
	}

	[HttpGet]
	public async Task<ActionResult<Basket>> GetBasketByUserId([FromQuery] int userId)
	{
		if (!await EnsureUserExists(userId))
			return BadRequest();

		var basket = await _context.Baskets
			.FirstOrDefaultAsync(u => u.UserId == userId);

		if (basket is null)
			return NotFound();

		return Ok(basket);
	}

	[HttpPost]
	public async Task<ActionResult<Basket>> CreateBasket([FromBody] int userId)
	{
		if(!await EnsureUserExists(userId))
			return BadRequest();

		Basket newBasket = new()
		{
			UserId = userId,
		};

		await _context.Baskets.AddAsync(newBasket);
		await _context.SaveChangesAsync();

		var entityChangedMessage = new EntityChangedMessage()
		{
			EntityName = "Basket",
			EntityId = newBasket.Id,
			ChangeType = EntityChangeType.Created,
			NewValue = JsonSerializer.Serialize(newBasket)
		};
		_channel.BasicPublish(_rabbitMqOptions.EntityExchange, "create", null,
			Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entityChangedMessage)));
		_logger.LogInformation($"Basket with id {userId} was created.");

		return Ok(newBasket);
	}

	[HttpPost("{id:int}/addProduct")]
	public async Task<ActionResult<Basket>> AddProductsToBasket(int id, [FromBody] int[] productIds)
	{
		var basket = await _context.Baskets.FirstOrDefaultAsync(b => b.Id == id);

		if (basket is null ||
			!await EnsureProductIdsExists(productIds))
			return BadRequest();

		basket.BasketProductIds?.AddRange(productIds);

		_context.Entry(basket).State = EntityState.Modified;

		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!BasketExists(id))
				return NotFound();
			else
				throw;
		}

		var entityChangedMessage = new EntityChangedMessage()
		{
			EntityName = "Basket",
			EntityId = basket.Id,
			ChangeType = EntityChangeType.Created,
			NewValue = JsonSerializer.Serialize(basket)
		};
		_channel.BasicPublish(_rabbitMqOptions.EntityExchange, "update", null,
			Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entityChangedMessage)));
		_logger.LogInformation($"Product with ids {string.Join(',', productIds)} were(was) added to the basket with id {id}.");

		return Ok();
	}

	[HttpPut("{id:int}")]
	public async Task<IActionResult> UpdateBasket(int id, [FromBody] Basket basketToUpdate)
	{
		if (id != basketToUpdate.Id)
			return BadRequest();

		_context.Entry(basketToUpdate).State = EntityState.Modified;

		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			if (BasketExists(id))
				return NotFound();
			throw;
		}

		var entityChangedMessage = new EntityChangedMessage()
		{
			EntityName = "Basket",
			EntityId = basketToUpdate.Id,
			ChangeType = EntityChangeType.Created,
			NewValue = JsonSerializer.Serialize(basketToUpdate)
		};
		_channel.BasicPublish(_rabbitMqOptions.EntityExchange, "update", null,
			Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entityChangedMessage)));
		_logger.LogInformation($"Basket with id {id} was updated.");

		return Ok();
	}

	[HttpDelete("{id:int}")]
	public async Task<IActionResult> DeleteBasket(int id)
	{
		var basket = await _context.Baskets.FirstOrDefaultAsync(u => u.Id == id);

		if (basket is null)
			return NotFound();

		_context.Baskets.Remove(basket);
		await _context.SaveChangesAsync();

		var entityChangedMessage = new EntityChangedMessage()
		{
			EntityName = "Basket",
			EntityId = basket.Id,
			ChangeType = EntityChangeType.Created,
			NewValue = JsonSerializer.Serialize(basket)
		};
		_channel.BasicPublish(_rabbitMqOptions.EntityExchange, "delete", null,
			Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entityChangedMessage)));
		_logger.LogInformation($"Basket with id {id} was deleted.");

		return Ok();
	}

	private async Task<bool> EnsureUserExists(int userId)
	{
		string uri = _configuration.GetValue<string>("ServiceUrls:UsersService");

		var response = await _httpClient.GetAsync(uri + $"/{userId}");

		if (!response.IsSuccessStatusCode)
			return false;

		return true;
	}

	private async Task<bool> EnsureProductIdsExists(int[] productIds)
	{
		string uri = _configuration.GetValue<string>("ServiceUrls:StoreService");

		foreach (int id in productIds)
		{
			var response = await _httpClient.GetAsync(uri + $"/{id}");

			if (!response.IsSuccessStatusCode)
				return false;
		}

		return true;
	}

	private bool BasketExists(int id)
	{
		return (_context.Baskets?.Any(e => e.Id == id))
			.GetValueOrDefault();
	}
}
