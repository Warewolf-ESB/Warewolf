using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Communication;
using Microsoft.Azure.SignalR.Management;

namespace Warewolf.Runtime.AzureFunctions;

/// <summary>
/// IDebugWriter implementation for the Azure Functions deployment.
/// Broadcasts Warewolf debug states to all clients connected to the Azure SignalR
/// Service "esb" hub using the server-side Management SDK — the same hub and
/// "SendDebugState" method that Studio already handles when talking to the regular
/// web server via EsbHub.
///
/// The instance is created once at startup (see Program.cs) and registered with
/// DebugDispatcher so that every workflow execution whose IsDebug flag is enabled
/// will stream debug states here as activities complete, rather than waiting until
/// the entire workflow finishes.
/// </summary>
public sealed class AzureSignalRDebugWriter : IDebugWriter, IAsyncDisposable
{
    private readonly ServiceHubContext _hubContext;
    private readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();

    private AzureSignalRDebugWriter(ServiceHubContext hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Creates and connects a hub context to the Azure SignalR Service.
    /// Call once at startup; the returned instance is long-lived.
    /// </summary>
    public static async Task<AzureSignalRDebugWriter> CreateAsync(
        string connectionString,
        CancellationToken ct = default)
    {
        var serviceManager = new ServiceManagerBuilder()
            .WithOptions(o => o.ConnectionString = connectionString)
            .BuildServiceManager();

        var hubContext = await serviceManager.CreateHubContextAsync("esb", ct);
        return new AzureSignalRDebugWriter(hubContext);
    }

    /// <inheritdoc/>
    /// Called by DebugDispatcher when an IDebugState object is available directly
    /// (e.g. from StateNotifier listeners that have a reference to the object).
    public void WriteDebugState(IDebugState debugState)
    {
        var serialized = _serializer.Serialize(debugState);
        // Fire-and-forget: don't block the workflow execution thread.
        _ = _hubContext.Clients.All.SendCoreAsync("SendDebugState", new object[] { serialized });
    }

    /// <inheritdoc/>
    /// Called by DebugDispatcher after each activity's debug state is ready —
    /// the string is already serialized JSON, matching what EsbHub.Write() receives.
    public void Write(string serializedDebugState)
    {
        // Fire-and-forget: don't block the workflow execution thread.
        _ = _hubContext.Clients.All.SendCoreAsync("SendDebugState", new object[] { serializedDebugState });
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubContext != null)
            await _hubContext.DisposeAsync();
    }
}
