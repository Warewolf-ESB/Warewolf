namespace WwExecutionCaller;

/// <summary>
/// Typed client abstraction over the Warewolf Execution Engine's authenticated routes.
/// Implementations rely on the registered <see cref="Auth.WwExecutionTokenHandler"/> to
/// attach the <c>Authorization: Bearer</c> (and, for /services, <c>x-functions-key</c>) headers.
/// </summary>
public interface IWwExecutionDownstreamService
{
    /// <summary>
    /// Invokes a workflow on the engine's Bearer-protected <c>/secure/{workflow}.json</c> route.
    /// </summary>
    /// <param name="workflow">Workflow name, URL-decoded (e.g. <c>Hello World</c>).</param>
    /// <param name="queryString">Optional raw query string (with or without a leading <c>?</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The engine's status code and response body.</returns>
    Task<WwExecutionResult> ExecuteSecureAsync(
        string workflow,
        string? queryString = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes a workflow on the engine's <c>/services/{workflow}.json</c> route, which requires
    /// both a Bearer token and an <c>x-functions-key</c> header.
    /// </summary>
    /// <param name="workflow">Workflow name, URL-decoded.</param>
    /// <param name="queryString">Optional raw query string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The engine's status code and response body.</returns>
    Task<WwExecutionResult> ExecuteServicesAsync(
        string workflow,
        string? queryString = null,
        CancellationToken cancellationToken = default);
}

/// <summary>Result of a downstream engine call: HTTP status code and raw body.</summary>
/// <param name="StatusCode">HTTP status code returned by the engine.</param>
/// <param name="ContentType">Response media type (defaults to <c>application/json</c>).</param>
/// <param name="Body">Raw response body.</param>
public readonly record struct WwExecutionResult(
    int StatusCode,
    string ContentType,
    string Body)
{
    /// <summary>True for 2xx responses.</summary>
    public bool IsSuccess => StatusCode is >= 200 and < 300;
}
