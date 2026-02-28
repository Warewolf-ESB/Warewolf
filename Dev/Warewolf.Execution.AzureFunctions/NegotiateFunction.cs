using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.SignalRService;
using Microsoft.Azure.Functions.Worker.Http;

namespace Warewolf.Execution.AzureFunctions;

/// <summary>
/// Negotiate endpoint — Warewolf Studio calls POST /api/negotiate to receive
/// the Azure SignalR Service URL and a short-lived JWT access token.
/// Studio then connects directly to the SignalR Service using those credentials;
/// the Functions app never holds the persistent WebSocket connection.
///
/// The hub name "esb" matches the existing EsbHub used by the regular web server,
/// so Studio can use the same SignalR client logic regardless of whether it is
/// connected to an on-premises server or an Azure Functions deployment.
///
/// Typical flow:
///   1. Studio: POST /api/negotiate
///   2. Function returns { url, accessToken } (SignalRConnectionInfo)
///   3. Studio connects to Azure SignalR Service with those credentials
///   4. Functions app broadcasts debug states via AzureSignalRDebugWriter during
///      each workflow execution; Studio receives them as "SendDebugState" messages
/// </summary>
public class NegotiateFunction
{
    [Function("negotiate")]
    public SignalRConnectionInfo Negotiate(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        HttpRequestData req,
        [SignalRConnectionInfoInput(HubName = "esb")]
        SignalRConnectionInfo connectionInfo)
    {
        return connectionInfo;
    }
}
