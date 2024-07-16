using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OnlineShop.Email.Console;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddOptions<RabbitMQOptions>()
    .Bind(config.GetSection(RabbitMQOptions.SectionName));

var serviceProvider = serviceCollection.BuildServiceProvider();
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

channel.ExchangeDeclare(rabbitMqOptions.EmailExchange, "fanout" , false, false, null);
		
channel.QueueDeclare(rabbitMqOptions.EmailSendQueue, false, false, false, null);
channel.QueueBind(rabbitMqOptions.EmailSendQueue, rabbitMqOptions.EmailExchange, "send");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, eventArgs) =>
{
    var email = JsonSerializer.Deserialize<Email>(eventArgs.Body.Span);
    SendEmail(email);
};
    
channel.BasicConsume(
    queue: rabbitMqOptions.EmailSendQueue,
    autoAck: true,
    consumer: consumer);

Console.ReadLine();

void SendEmail(Email email)
{
    Console.WriteLine($"Email to {email.To} with subject {email.Subject} and body {email.Body} has been sent.");
}