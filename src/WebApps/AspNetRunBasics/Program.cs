using AspnetRunBasics.Services;
using Common.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<LoggingDelegatingHandler>();

// Add services to the container.
builder.Services.AddHttpClient<ICatalogService, CatalogService>(c =>
#pragma warning disable CS8604 // Possible null reference argument.
                c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
#pragma warning restore CS8604 // Possible null reference argument.
                .AddHttpMessageHandler<LoggingDelegatingHandler>();

builder.Services.AddHttpClient<IBasketService, BasketService>(c =>
#pragma warning disable CS8604 // Possible null reference argument.
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
#pragma warning restore CS8604 // Possible null reference argument.
                .AddHttpMessageHandler<LoggingDelegatingHandler>();

builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
#pragma warning disable CS8604 // Possible null reference argument.
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
#pragma warning restore CS8604 // Possible null reference argument.
                .AddHttpMessageHandler<LoggingDelegatingHandler>();

builder.Services.AddRazorPages();

builder.Host.UseSerilog(SeriLogger.Configure);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
