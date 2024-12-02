using Card.API.Kafka;
using Card.Infrastructure.Kafka;
using Cart.API.Services;
using Cart.Infrastructure.Kafka;
using Cart.Infrastructure.Repositories;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add the producer service as singletons:
builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

builder.Services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();
// Add the kafka consumer service as a hosted service (background service that runs for the lifetime of the application):
builder.Services.AddHostedService<KafkaConsumer>();
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddSingleton<CartService>();

// Add redis
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddSingleton<ICartRepository, RedisCartRepository>();

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
