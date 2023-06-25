using Common.Logging;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Shopping.Aggregator.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

#pragma warning disable CS8604 // Possible null reference argument.
        builder.Services.AddHttpClient<ICatalogService, CatalogService>(c =>
                        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:CatalogUrl"]))
                        .AddPolicyHandler(GetRetryPolicy())
                        .AddPolicyHandler(GetCircuitBreakerPolicy());

        builder.Services.AddHttpClient<IBasketService, BasketService>(c =>
                        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BasketUrl"]))
                        .AddPolicyHandler(GetRetryPolicy())
                        .AddPolicyHandler(GetCircuitBreakerPolicy());

        builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
                        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:OrderingUrl"]))
                        .AddPolicyHandler(GetRetryPolicy())
                        .AddPolicyHandler(GetCircuitBreakerPolicy());
#pragma warning restore CS8604 // Possible null reference argument.

        builder.UseEnrichedSerilog();

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
    }
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        // In this case will wait for
        //  2 ^ 1 = 2 seconds then
        //  2 ^ 2 = 4 seconds then
        //  2 ^ 3 = 8 seconds then
        //  2 ^ 4 = 16 seconds then
        //  2 ^ 5 = 32 seconds

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, retryCount, context) =>
                {
                    Log.Error($"Retry {retryCount} of {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30)
            );
    }
}