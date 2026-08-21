namespace Warewolf.Execution.ServiceBusWorker;

/// <summary>
/// Typed client for the Warewolf Execution Engine HTTP API.
///
/// Implementations are wired with the <c>WwExecutionTokenHandler</c>, so callers never deal
/// with tokens — authentication and the <c>x-functions-key</c> header are injected automatically.
/// </summary>
public interface IWwExecutionClient
{
    /// <summary>
    /// Executes a workflow on the secured <c>/secure/{workflow}.json</c> route (Entra Bearer token).
    /// </summary>
    /// <param name="workflow">Workflow name, e.g. <c>Hello World</c> (spaces are URL-encoded for you).</param>
    /// <param name="query">Workflow inputs sent as query-string parameters (may be empty/null).</param>
    /// <returns>The raw JSON response body from the engine.</returns>
    Task<string> ExecuteSecureAsync(
        string workflow,
        IDictionary<string, string?>? query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a workflow on the <c>/services/{workflow}.json</c> route, which requires both
    /// the Bearer token AND a function key (<c>x-functions-key</c>). The key must be configured
    /// via <see cref="WwExecutionOptions.FunctionKey"/>.
    /// </summary>
    Task<string> ExecuteServiceAsync(
        string workflow,
        IDictionary<string, string?>? query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a workflow on the anonymous <c>/public/{workflow}.json</c> route (no token).
    /// </summary>
    Task<string> ExecutePublicAsync(
        string workflow,
        IDictionary<string, string?>? query,
        CancellationToken cancellationToken = default);
}
