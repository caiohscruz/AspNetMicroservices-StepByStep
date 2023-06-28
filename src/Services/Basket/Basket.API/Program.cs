using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Common.Logging;
using Discount.Grpc.Protos;
using HealthChecks.UI.Client;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ServiceStack;
using ServiceStack.Redis;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Redis Configuration
builder.Services.AddSingleton<IRedisClientsManager>(c =>
{
    var redisConnectionString = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString");
    return new RedisManagerPool(redisConnectionString);
});

// General Configuration
builder.Services.AddScoped<IBasketRepository, BasketRepository>();

// AutoMapper Configuration
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

// Grpc Configuration
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(
    options =>
    {
        var discountUrl = builder.Configuration["GrpcSettings:DiscountUrl"];
        options.Address = new Uri(discountUrl);
    });
builder.Services.AddScoped<DiscountGrpcService>();

// MassTransit-RabbitMQ Configuration
builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((ctx, cfg) => {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
    });
});

builder.UseEnrichedSerilog();

builder.Services.AddHealthChecks()
                    .AddRedis(builder.Configuration["CacheSettings:ConnectionString"], "Redis Health", HealthStatus.Degraded);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
