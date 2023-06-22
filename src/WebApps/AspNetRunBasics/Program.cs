using AspnetRunBasics.Services;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<ICatalogService, CatalogService>(c =>
#pragma warning disable CS8604 // Possible null reference argument.
                c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]));
#pragma warning restore CS8604 // Possible null reference argument.

builder.Services.AddHttpClient<IBasketService, BasketService>(c =>
#pragma warning disable CS8604 // Possible null reference argument.
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]));
#pragma warning restore CS8604 // Possible null reference argument.

builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
#pragma warning disable CS8604 // Possible null reference argument.
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]));
#pragma warning restore CS8604 // Possible null reference argument.

builder.Services.AddRazorPages();

builder.Host.UseSerilog((ctx, config) =>
{
    var appName = Assembly.GetExecutingAssembly().GetName().Name;
    var environment = ctx.HostingEnvironment.EnvironmentName;

    config
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(
#pragma warning disable CS8604 // Possible null reference argument.
            new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(ctx.Configuration["ElasticConfiguration:Uri"]))
#pragma warning restore CS8604 // Possible null reference argument.
            {
                IndexFormat = $"applogs-{appName?.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}",
                AutoRegisterTemplate = true,
                NumberOfShards = 3,
                NumberOfReplicas = 1,
            }
        )
#pragma warning disable CS8604 // Possible null reference argument.
        .Enrich.WithProperty("Environment", environment)
#pragma warning restore CS8604 // Possible null reference argument.
        .ReadFrom.Configuration(ctx.Configuration);
});

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
