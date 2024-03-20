using BlazorApp1.Components;
using Elsa.Studio.Contracts;
using Elsa.Studio.Core.BlazorWasm.Extensions;
using Elsa.Studio.Extensions;
using Elsa.Studio.Host.CustomElements.HttpMessageHandlers;
using Elsa.Studio.Host.CustomElements.Services;
using Elsa.Studio.Shell.Extensions;
using Elsa.Studio.Workflows.Designer.Extensions;
using Elsa.Studio.Workflows.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(circuitOptions => circuitOptions.RootComponents.RegisterCustomElsaStudioElements());

// Register local services.
builder.Services.AddSingleton<BackendService>();

// Register the modules.
builder.Services.AddCore();
builder.Services.AddShell();
builder.Services.AddRemoteBackend(
    elsaClient =>
    {
        elsaClient.ApiKey = configuration["Backend:ApiKey"];
        elsaClient.AuthenticationHandler = typeof(AuthHttpMessageHandler);
    },
    options => configuration.GetSection("Backend").Bind(options));
builder.Services.Replace(ServiceDescriptor.Scoped<IRemoteBackendAccessor, ComponentRemoteBackendAccessor>());
builder.Services.AddWorkflowsModule();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();