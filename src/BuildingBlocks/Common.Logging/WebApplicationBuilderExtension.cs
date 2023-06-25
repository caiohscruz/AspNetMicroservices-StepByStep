using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace Common.Logging
{
    public static class WebApplicationBuilderExtension
    {
        public static WebApplicationBuilder UseEnrichedSerilog(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<LoggingDelegatingHandler>();

            builder.Services.ConfigureAll<HttpClientFactoryOptions>(options =>
                options.HttpMessageHandlerBuilderActions.Add(handlerBuilder =>
                    handlerBuilder.AdditionalHandlers.Add(handlerBuilder.Services.GetRequiredService<LoggingDelegatingHandler>())));

            var logger = new LoggerConfiguration();

            logger.Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(
                        new Uri(builder.Configuration["ElasticConfiguration:Uri"]))
                    {
                        IndexFormat =
                            $"applogs-{Assembly.GetEntryAssembly().GetName().Name!.ToLower().Replace(".", "-")}-{builder.Environment.EnvironmentName?.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}",
                        AutoRegisterTemplate = true,
                        NumberOfShards = 2,
                        NumberOfReplicas = 1,
                    })
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                .ReadFrom.Configuration(builder.Configuration);

            builder.Logging.AddSerilog(logger.CreateLogger());
            return builder;
        }
    }
}
