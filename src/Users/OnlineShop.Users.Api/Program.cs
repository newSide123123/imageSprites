using Microsoft.EntityFrameworkCore;
using OnlineShop.Users.Api.Controllers;
using OnlineShop.Users.Api.EF;
using OnlineShop.Users.Api.Options;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddDbContext<UsersDbContext>(options =>
	options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

Log.Logger = new LoggerConfiguration()
		.MinimumLevel.Debug()
		.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
		.Enrich.FromLogContext()
		.WriteTo.File(new JsonFormatter(), "Logs\\log-.txt", rollingInterval: RollingInterval.Day)
		.CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<ServiceUrls>()
	.Bind(configuration.GetSection(ServiceUrls.SectionName));
builder.Services.AddOptions<RabbitMQOptions>()
	.Bind(configuration.GetSection(RabbitMQOptions.SectionName));

builder.Services.AddHttpClient<UsersController>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();