using AzureFunctions.ProfilerServices.Domain.Interfaces;
using AzureFunctions.ProfilerServices.Domain.Services;
using AzureFunctions.ProfilerServices.Utility;
using Elastic.Apm.Azure.Functions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Elastic APM tracing middleware - traces all function invocations.
builder.UseMiddleware<ApmMiddleware>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Skip external sinks during OpenAPI generation in CI.
var generatingOpenApi = string.Equals(
    Environment.GetEnvironmentVariable("OPENAPI_GEN"),
    "true",
    StringComparison.OrdinalIgnoreCase);

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext();

if (!generatingOpenApi
    && !string.IsNullOrWhiteSpace(GlobalVariables.ElasticSearchHost)
    && Uri.IsWellFormedUriString(GlobalVariables.ElasticSearchHost, UriKind.Absolute))
{
    loggerConfig = loggerConfig.WriteTo.Elasticsearch(
        new ElasticsearchSinkOptions(new Uri(GlobalVariables.ElasticSearchHost))
        {
            IndexFormat = GlobalVariables.ElasticConfigurationIndexName,
            AutoRegisterTemplate = false,
            ModifyConnectionSettings = x => x.BasicAuthentication(
                GlobalVariables.ElasticSearchUsername,
                GlobalVariables.ElasticSearchPassword)
        });
}

var logger = loggerConfig.CreateLogger();

Log.CloseAndFlush();

// Register Serilog
builder.Services.AddSerilog(logger, dispose: true);

builder.UseMiddleware<ExceptionLoggingMiddleware>();
builder.Services.AddSingleton<ExceptionLoggingMiddleware>();

builder.Services.AddSingleton<IOpenApiConfigurationOptions, OpenApiConfigurationOptions>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IProfilerHttpService, ProfilerHttpService>();

builder.Build().Run();

