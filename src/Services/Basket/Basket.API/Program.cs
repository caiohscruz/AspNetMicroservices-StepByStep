using Basket.API.Repositories;
using ServiceStack;
using ServiceStack.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRedisClientsManager>(c =>
{
    var redisConnectionString = builder.Configuration.GetValue<string>("CacheSettings:ConnectionString");
    return new RedisManagerPool(redisConnectionString);
});

builder.Services.AddScoped<IBasketRepository, BasketRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
