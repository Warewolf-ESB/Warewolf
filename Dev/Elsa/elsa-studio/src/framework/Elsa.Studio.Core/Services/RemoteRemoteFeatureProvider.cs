using System.Net;
using Elsa.Api.Client.Resources.Features.Contracts;
using Elsa.Api.Client.Resources.Features.Models;
using Elsa.Studio.Contracts;
using Refit;

namespace Elsa.Studio.Services;

/// <summary>
/// A feature service that uses a remote backend to retrieve feature flags.
/// </summary>
public class RemoteRemoteFeatureProvider : IRemoteFeatureProvider
{
    private readonly IRemoteBackendApiClientProvider _remoteBackendApiClientProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBlazorServiceAccessor _blazorServiceAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteRemoteFeatureProvider"/> class.
    /// </summary>
    public RemoteRemoteFeatureProvider(IRemoteBackendApiClientProvider remoteBackendApiClientProvider, IServiceProvider serviceProvider, IBlazorServiceAccessor blazorServiceAccessor)
    {
        _remoteBackendApiClientProvider = remoteBackendApiClientProvider;
        _serviceProvider = serviceProvider;
        _blazorServiceAccessor = blazorServiceAccessor;
    }

    /// <inheritdoc />
    public async Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        _blazorServiceAccessor.Services = _serviceProvider;
        var api = await _remoteBackendApiClientProvider.GetApiAsync<IFeaturesApi>(cancellationToken);

        try
        {
            _ = await api.GetAsync(featureName, cancellationToken);
            return true;
        }
        catch (ApiException e) when(e.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FeatureDescriptor>> ListAsync(CancellationToken cancellationToken = default)
    {
        var api = await _remoteBackendApiClientProvider.GetApiAsync<IFeaturesApi>(cancellationToken);
        var response = await api.ListAsync(cancellationToken);
        return response.Items;
    }
}