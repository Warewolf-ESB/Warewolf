using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace AzureFunctions.ProfilerServices.Utility;

public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new()
    {
        Version = "1.0.0",
        Title = "Profiler Services API",
        Description = "HTTP APIs exposed by the Profiler Services Azure Functions for APIM consumption."
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;

    public override bool ForceHttps { get; set; } = false;
}
