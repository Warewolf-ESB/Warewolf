using Azure.Core;
using Azure.Identity;
using AzureFunctions.ProfilerServices.Domain.Interfaces;
using AzureFunctions.ProfilerServices.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AzureFunctions.ProfilerServices.Domain.Services
{
    public class ProfilerHttpService : IProfilerHttpService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProfilerHttpService> _logger;

        public ProfilerHttpService(IHttpClientFactory httpClientFactory
                          , ILogger<ProfilerHttpService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUrl)
        {
            var client = _httpClientFactory.CreateClient();
            string Url = GlobalVariables.genericBaseURL + requestUrl;

            if (!GlobalVariables.usesEngineIdentity)
            {
                // VM-era deployment: the VM Warewolf server's static token, exactly as before the engine migration.
                client.DefaultRequestHeaders.Add("Authorization", GlobalVariables.genericSecureWarewolfAuthorizationToken);
                return await SendAsync(client, Url);
            }

            // Bearer token for the Execution Engine, fetched per call and not cached: the timer runs
            // weekly, so a stored token would have expired. The credential is chosen explicitly rather
            // than via DefaultAzureCredential's fall-through chain, so stray AZURE_CLIENT_* app settings
            // cannot swap the identity in Azure:
            //   Azure (IDENTITY_ENDPOINT or WEBSITE_INSTANCE_ID set) -> this Function App's system-assigned managed identity
            //   local development                                    -> the developer's `az login`
            var runningInAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("IDENTITY_ENDPOINT"))
                              || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
            TokenCredential credential = runningInAzure
                ? new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned)
                : new AzureCliCredential();

            try
            {
                var token = await credential.GetTokenAsync(
                    new TokenRequestContext(new[] { $"api://{GlobalVariables.genericEngineResourceAppId}/.default" }),
                    default);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            }
            catch (AuthenticationFailedException ex)
            {
                _logger.LogError(ex,
                    "Could not acquire an Execution Engine token. Check that the Function App's system-assigned " +
                    "managed identity is enabled and GenericEngineResourceAppId is the engine's client id " +
                    "(its App ID URI api://<id> must exist).");
                throw;
            }

            return await SendAsync(client, Url);
        }

        // The engine's status is logged here so a failed workflow call is visible whoever the caller is.
        private async Task<HttpResponseMessage> SendAsync(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
                _logger.LogInformation("{Url} answered {StatusCode}.", url, (int)response.StatusCode);
            else
                _logger.LogError("{Url} answered {StatusCode}.", url, (int)response.StatusCode);
            return response;
        }
    }
}
