using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OnlineShop.EntityHistory.Console;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddDbContext<EntityHistoryDbContext>(
    options => options.UseNpgsql(config.GetConnectionString("PostgresConnection")));
serviceCollection.AddOptions<RabbitMQOptions>()
    .Bind(config.GetSection(RabbitMQOptions.SectionName));
    
var serviceProvider = serviceCollection.BuildServiceProvider();

var dbContext = serviceProvider.GetRequiredService<EntityHistoryDbContext>();
var rabbitMqOptions = serviceProvider.GetRequiredService<IOptions<RabbitMQOptions>>().Value;

var factory = new ConnectionFactory
{
    HostName = rabbitMqOptions.Host,
    Port = rabbitMqOptions.Port,
    UserName = rabbitMqOptions.Username,
    Password = rabbitMqOptions.Password
};
var connection = factory.CreateConnection();
var channel = connection.CreateModel();
	
channel.ExchangeDeclare(rabbitMqOptions.EntityExchange, "direct" , false, false, null);
		
channel.QueueDeclare(rabbitMqOptions.EntityCreateQueue, false, false, false, null);
channel.QueueBind(rabbitMqOptions.EntityCreateQueue, rabbitMqOptions.EntityExchange, "create");

channel.QueueDeclare(rabbitMqOptions.EntityUpdateQueue, false, false, false, null);
channel.QueueBind(rabbitMqOptions.EntityUpdateQueue, rabbitMqOptions.EntityExchange, "update");
		
channel.QueueDeclare(rabbitMqOptions.EntityDeleteQueue, false, false, false, null);
channel.QueueBind(rabbitMqOptions.EntityDeleteQueue, rabbitMqOptions.EntityExchange, "delete");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += async (model, eventArgs) =>
{
    var message = JsonSerializer.Deserialize<EntityChangedMessage>(eventArgs.Body.Span);
    await dbContext.EntityChanges.AddAsync(message);
    await dbContext.SaveChangesAsync();
};
    
channel.BasicConsume(
    queue: rabbitMqOptions.EntityCreateQueue,
    autoAck: true,
    consumer: consumer);
        
channel.BasicConsume(
    queue: rabbitMqOptions.EntityUpdateQueue,
    autoAck: true,
    consumer: consumer);
        
channel.BasicConsume(
    queue: rabbitMqOptions.EntityDeleteQueue,
    autoAck: true,
    consumer: consumer);

Console.ReadLine();
    