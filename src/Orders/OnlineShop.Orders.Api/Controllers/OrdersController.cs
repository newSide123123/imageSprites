using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlineShop.Orders.Api.EF;
using OnlineShop.Orders.Api.Enums;
using OnlineShop.Orders.Api.Models;
using OnlineShop.Orders.Api.Options;
using RabbitMQ.Client;
using System.Text;

namespace OnlineShop.Orders.Api.Controllers
{
	[Route("api/orders")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
        private readonly OrdersDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ServiceUrls _serviceUrls; 
        private readonly RabbitMQOptions _rabbitMqOptions;
        private readonly IModel _channel;
		private readonly ILogger<OrdersController> _logger;

		public OrdersController(OrdersDbContext context, HttpClient httpClient,
            IOptions<ServiceUrls> serviceUrls, IOptions<RabbitMQOptions> rabbitMqOptions, ILogger<OrdersController> logger)
        {
            _context = context;
            var items = new List<Order>
            {
                new Order {
                    UserId = 1,
                    OrderDate = DateTime.UtcNow,
                    Items = new List<OrderItem> {
                        new OrderItem {
                            Price = 12999.99m,
                            ProductId = 1,
                            Amount = 1}
                        },
                    TotalPrice = 12999.99m
                },
                new Order {
                    UserId = 1,
                    OrderDate = DateTime.UtcNow,
                    Items = new List<OrderItem> {
                        new OrderItem
                        {
                            Price = 4700m,
                            ProductId = 2,
                            Amount = 2
                        }
                    },
                    TotalPrice = 9400m },
                new Order {
                    UserId = 2,
                    OrderDate = DateTime.UtcNow,
                    Items = new List<OrderItem> {
                        new OrderItem
                        {
                            Price = 39999.99m,
                            ProductId = 13,
                            Amount = 1
                        }
                    },
                    TotalPrice = 39999.99m },
            };
            _context.AddRange(items);
            _context.SaveChanges();
            _httpClient = httpClient;
            _serviceUrls = serviceUrls.Value; 
            _rabbitMqOptions = rabbitMqOptions.Value;

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
		public async Task<IActionResult> GetOrders()
		{
			return Ok(await _context.Orders
                .Include(o => o.Items).ToListAsync());
		}

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            return Ok(await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id) ?? throw new ArgumentException());
        }

        [HttpPost]
		public async Task<IActionResult> CreateOrder([FromBody] Order order)
		{
            var response = await _httpClient.GetAsync(_serviceUrls.UsersService + $"/{order.UserId}");

            if (!response.IsSuccessStatusCode)
                return BadRequest(await response.Content.ReadAsStringAsync());

            var res = await _context.Orders.AddAsync(order);
			await _context.SaveChangesAsync();
            var responseData = await response.Content.ReadAsStringAsync();
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(responseData);

            var email = new Models.Email()
            {
                To = user.Email,
                Subject = "Your order is successfuly created!",
                Body = $"Hi, {user.FirstName} {user.LastName}! Your order cost {order.TotalPrice}. For details go to your cabinet."
            };
            _channel.BasicPublish(_rabbitMqOptions.EmailExchange, "send", null,
                Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(email)));

            var entityChangedMessage = new EntityChangedMessage()
            {
                EntityName = "Orders",
                EntityId = res.Entity.Id,
                ChangeType = EntityChangeType.Created,
                NewValue = System.Text.Json.JsonSerializer.Serialize(user)
            };
            _channel.BasicPublish(_rabbitMqOptions.EntityExchange, "create", null,
                Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(entityChangedMessage)));
            _logger.LogInformation($"Order with id {res.Entity.Id} was created.");

            return Ok(res.Entity);
		}

        [HttpDelete]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == orderId) ?? throw new ArgumentException();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            var response = await _httpClient.GetAsync(_serviceUrls.UsersService + $"/{order.UserId}");
            var responseData = await response.Content.ReadAsStringAsync();
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(responseData);

            var email = new Models.Email()
            {
                To = user.Email,
                Subject = "Your order is successfuly canceled!",
                Body = $"Hi, {user.FirstName} {user.LastName}! Your order {order.Id} is canceled. For details go to your cabinet."
            };
            _channel.BasicPublish(_rabbitMqOptions.EmailExchange, "send", null,
                Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(email)));
            _logger.LogInformation($"Order with id {orderId} was deleted.");

            var entityChangedMessage = new EntityChangedMessage()
            {
                EntityName = "Orders",
                EntityId = order.Id,
                ChangeType = EntityChangeType.Deleted,
                NewValue = System.Text.Json.JsonSerializer.Serialize(user)
            };
            _channel.BasicPublish(_rabbitMqOptions.EntityExchange, "delete", null,
                Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(entityChangedMessage)));


            return Ok();
        }
    }
}
