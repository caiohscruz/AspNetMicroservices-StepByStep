using AspnetRunBasics.Services;
using Common.Logging;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using Polly.Extensions.Http;
using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddHttpClient<ICatalogService, CatalogService>(c =>
#pragma warning disable CS8604 // Possible null reference argument.
                        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
                        .AddPolicyHandler(GetRetryPolicy())
                        .AddPolicyHandler(GetCircuitBreakerPolicy());

        builder.Services.AddHttpClient<IBasketService, BasketService>(c =>
                        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
                        .AddPolicyHandler(GetRetryPolicy())
                        .AddPolicyHandler(GetCircuitBreakerPolicy());

        builder.Services.AddHttpClient<IOrderService, OrderService>(c =>
                        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]))
                        .AddPolicyHandler(GetRetryPolicy())
                        .AddPolicyHandler(GetCircuitBreakerPolicy());
#pragma warning restore CS8604 // Possible null reference argument.

        builder.Services.AddRazorPages();

        builder.UseEnrichedSerilog();

        builder.Services.AddHealthChecks()
                .AddUrlGroup(new Uri(builder.Configuration["ApiSettings:GatewayAddress"]), "Ocelot API Gw", HealthStatus.Degraded);

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

        app.MapHealthChecks("/hc", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

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