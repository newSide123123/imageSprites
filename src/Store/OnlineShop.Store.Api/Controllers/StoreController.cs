using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineShop.Store.Api.EF;
using OnlineShop.Store.Api.Models;
using OnlineShop.Store.Api.Options;
using RabbitMQ.Client;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using OnlineShop.Store.Api.Enums;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace OnlineShop.Store.Api.Controllers
{
	[Route("api/store")]
	[ApiController]
	public class StoreController : ControllerBase
	{
		private readonly StoreDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly RabbitMQOptions _rabbitMqOptions;
        private readonly ServiceUrls _serviceUrls;
        private readonly IModel _channel;
		private readonly ILogger<StoreController> _logger;

		public StoreController(StoreDbContext context, HttpClient httpClient, IOptions<ServiceUrls> serviceUrls,
        IOptions<RabbitMQOptions> rabbitMqOptions, ILogger<StoreController> logger)
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

            channel.ExchangeDeclare(_rabbitMqOptions.EmailExchange, "fanout", false, false, null);

            channel.QueueDeclare(_rabbitMqOptions.EmailSendQueue, false, false, false, null);
            channel.QueueBind(_rabbitMqOptions.EmailSendQueue, _rabbitMqOptions.EmailExchange, "send");

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
		public async Task<IActionResult> GetAllProducts()
		{
			var products = await _context.Products.ToListAsync();
			return Ok(products);
		}

		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetProductById(int id)
		{
			var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

			if (product is null)
			{
				return NotFound();
			}
			return Ok(product);
		}

		[HttpPost]
		public async Task<IActionResult> AddProduct([FromBody] Product product)
		{
			await _context.Products.AddAsync(product);
			await _context.SaveChangesAsync();

            var entityChangedMessage = new EntityChangedMessage()
            {
                EntityName = "Store",
                EntityId = product.Id,
                ChangeType = EntityChangeType.Created,
                NewValue = JsonSerializer.Serialize(product)
            };
            _channel.BasicPublish(_rabbitMqOptions.EntityExchange, "create", null,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entityChangedMessage)));

			_logger.LogInformation($"Product with id {product.Id} was added.");

            return Ok(product);
		}

		[HttpPut("{id:int}")]
		public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
		{
			if (id != product.Id)
			{
				return BadRequest();
			}

			_context.Entry(product).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ProductExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

            var entityChangedMessage = new EntityChangedMessage()
            {
                EntityName = "Store",
                EntityId = id,
                ChangeType = EntityChangeType.Updated,
                NewValue = JsonSerializer.Serialize(product)
            };
            _channel.BasicPublish(_rabbitMqOptions.EntityExchange, "update", null,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entityChangedMessage)));

			_logger.LogInformation($"Product with id {id} was updated.");

            return Ok(product);
		}

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> DeleteProduct(int id)
		{
            var response = await _httpClient.GetAsync(_serviceUrls.BasketsService + $"/{id}");

            if (!response.IsSuccessStatusCode)
                return BadRequest(await response.Content.ReadAsStringAsync());

            // Parse the response content to check if the product exists in any basket
            var content = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<dynamic>(content);
            bool existsInBasket = responseObject.existsInBasket;

            if (existsInBasket)
            {
                // If the product exists in any basket, return a BadRequest
                return BadRequest("Product exists in a basket and cannot be deleted.");
            }     

            var product = await _context.Products.FirstOrDefaultAsync(u => u.Id == id);

			if (product is null)
			{
				return NotFound();
			}
			_context.Products.Remove(product);
			await _context.SaveChangesAsync();

            var entityChangedMessage = new EntityChangedMessage()
            {
                EntityName = "Store",
                EntityId = id,
                ChangeType = EntityChangeType.Deleted
            };
            _channel.BasicPublish(_rabbitMqOptions.EntityExchange, "delete", null,
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entityChangedMessage)));

            _logger.LogInformation($"Product with id {id} was deleted.");

            return Ok();
		}

		private bool ProductExists(int id)
		{
			return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
		}
	}
}