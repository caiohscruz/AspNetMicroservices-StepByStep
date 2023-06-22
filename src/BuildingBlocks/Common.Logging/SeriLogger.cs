﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Common.Logging
{
    public static class SeriLogger
    {
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
           (context, configuration) =>
           {
               var elasticUri = context.Configuration.GetValue<string>("ElasticConfiguration:Uri");

               configuration
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .WriteTo.Debug()
                    .WriteTo.Console()
                    .WriteTo.Elasticsearch(
#pragma warning disable CS8604 // Possible null reference argument.
                        new ElasticsearchSinkOptions(new Uri(elasticUri))
#pragma warning restore CS8604 // Possible null reference argument.
                        {
                            IndexFormat = $"applogs-{context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-")}-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                            AutoRegisterTemplate = true,
                            NumberOfShards = 2,
                            NumberOfReplicas = 1
                        })
#pragma warning disable CS8604 // Possible null reference argument.
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
#pragma warning restore CS8604 // Possible null reference argument.
                    .ReadFrom.Configuration(context.Configuration);
           };
    }
}