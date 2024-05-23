using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineShop.Users.Api.EF;
using OnlineShop.Users.Api.Enums;
using OnlineShop.Users.Api.Models;
using OnlineShop.Users.Api.Options;
using RabbitMQ.Client;

namespace OnlineShop.Users.Api.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
	private readonly UsersDbContext _context;
	private readonly HttpClient _httpClient;
	private readonly RabbitMQOptions _rabbitMqOptions;
	private readonly ServiceUrls _serviceUrls;

	private readonly IModel _channel;
	private readonly ILogger<UsersController> _logger;

	public UsersController(UsersDbContext context, HttpClient httpClient, IOptions<ServiceUrls> serviceUrls, 
		IOptions<RabbitMQOptions> rabbitMqOptions, ILogger<UsersController> logger)
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

		channel.ExchangeDeclare(_rabbitMqOptions.EmailExchange, "fanout" , false, false, null);
		
		channel.QueueDeclare(_rabbitMqOptions.EmailSendQueue, false, false, false, null);
		channel.QueueBind(_rabbitMqOptions.EmailSendQueue, _rabbitMqOptions.EmailExchange, "send");
		
		channel.ExchangeDeclare(_rabbitMqOptions.EntityExchange, "direct" , false, false, null);
		
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
	public async Task<IActionResult> GetUsers()
	{
		var users = await _context.Users.ToListAsync();

		return Ok(users);
	}

	[HttpGet("{id:int}")]
	public async Task<IActionResult> GetUserById(int id)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

		if (user is null)
			return NotFound();

		return Ok(user);
	}

	[HttpPost]
	public async Task<IActionResult> AddUser([FromBody] User user)
	{
		await _context.Users.AddAsync(user);
		await _context.SaveChangesAsync();
		
		var response = await _httpClient.PostAsync(_serviceUrls.BasketsService,
			JsonContent.Create(new { userId = user.Id, basketProducts = Array.Empty<int>() }));

		if (!response.IsSuccessStatusCode)
		{
			_context.Users.Remove(user);
			await _context.SaveChangesAsync();
			return BadRequest(await response.Content.ReadAsStringAsync());
		}

		var email = new Email()
		{
			To = user.Email,
			Subject = "Welcome to Online Shop!",
			Body = $"Welcome, {user.FirstName} {user.LastName}!"
		};
		_channel.BasicPublish(_rabbitMqOptions.EmailExchange, "send", null, 
			Encoding.UTF8.GetBytes(JsonSerializer.Serialize(email)));

		var entityChangedMessage = new EntityChangedMessage()
		{
			EntityName = "Users",
			EntityId = user.Id,
			ChangeType = EntityChangeType.Created,
			NewValue = JsonSerializer.Serialize(user)
		};
		_channel.BasicPublish(_rabbitMqOptions.EntityExchange, "create", null,
			Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entityChangedMessage)));
		
		return Ok(user);
	}

	[HttpPut("{id:int}")]
	public async Task<IActionResult> UpdateUser(int id, [FromBody] User userToUpdate)
	{
		if (id != userToUpdate.Id)
			return BadRequest();

		_context.Entry(userToUpdate).State = EntityState.Modified;

		try
		{
			await _context.SaveChangesAsync();
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!UserExists(id))
				return NotFound();
			throw;
		}
		
		var entityChangedMessage = new EntityChangedMessage()
		{
			EntityName = "Users",
			EntityId = id,
			ChangeType = EntityChangeType.Updated,
			NewValue = JsonSerializer.Serialize(userToUpdate)
		};
		_channel.BasicPublish(_rabbitMqOptions.EntityExchange, "update", null,
			Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entityChangedMessage)));

		return Ok(userToUpdate);
	}

	[HttpDelete("{id:int}")]
	public async Task<IActionResult> DeleteUser(int id)
	{
		var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

		if (user is null)
			return NotFound();

		_context.Users.Remove(user);
		await _context.SaveChangesAsync();
		
		var entityChangedMessage = new EntityChangedMessage()
		{
			EntityName = "Users",
			EntityId = id,
			ChangeType = EntityChangeType.Deleted
		};
		_channel.BasicPublish(_rabbitMqOptions.EntityExchange, "delete", null,
			Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entityChangedMessage)));
		
		return Ok();
	}

	private bool UserExists(int id)
	{
		return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
	}
}