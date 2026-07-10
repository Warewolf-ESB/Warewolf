namespace WwExecutionWebMvc.Services;

/// <summary>
/// Abstraction over the Warewolf Execution Engine HTTP API.
/// Implementations are responsible for URI-encoding workflow names and for
/// attaching the correct credentials per route family:
/// <list type="bullet">
///   <item><c>/public/*</c>  — anonymous, no token.</item>
///   <item><c>/secure/*</c>  — delegated user Bearer token.</item>
///   <item><c>/services/*</c>— delegated user Bearer token AND an x-functions-key.</item>
/// </list>
/// </summary>
public interface IWwExecutionService
{
    /// <summary>Calls an anonymous public workflow: <c>GET /public/{workflow}.json</c>.</summary>
    Task<string> ExecutePublicAsync(string workflow, IDictionary<string, string?>? query = null, CancellationToken ct = default);

    /// <summary>
    /// Calls a secured workflow on the signed-in user's behalf:
    /// <c>GET /secure/{workflow}.json</c> with a delegated Bearer token.
    /// May throw <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// when interactive consent / re-auth is required.
    /// </summary>
    Task<string> ExecuteSecureAsync(string workflow, IDictionary<string, string?>? query = null, CancellationToken ct = default);

    /// <summary>
    /// Calls a function-key-protected workflow: <c>GET /services/{workflow}.json</c>
    /// with a delegated Bearer token AND the configured <c>x-functions-key</c> header.
    /// </summary>
    Task<string> ExecuteServiceAsync(string workflow, IDictionary<string, string?>? query = null, CancellationToken ct = default);

    /// <summary>Retrieves the engine discovery document: <c>GET /apis.json</c> (anonymous).</summary>
    Task<string> GetApisAsync(CancellationToken ct = default);
}
