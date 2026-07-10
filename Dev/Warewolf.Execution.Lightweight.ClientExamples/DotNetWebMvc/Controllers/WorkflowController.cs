using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using WwExecutionWebMvc.Services;

namespace WwExecutionWebMvc.Controllers;

/// <summary>
/// Demonstrates calling the engine on the signed-in user's behalf.
/// The whole controller requires authentication; the secure call triggers
/// silent token acquisition for the engine's delegated scope.
/// </summary>
[Authorize]
public sealed class WorkflowController : Controller
{
    private readonly IWwExecutionService _engine;
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(IWwExecutionService engine, ILogger<WorkflowController> logger)
    {
        _engine = engine;
        _logger = logger;
    }

    /// <summary>
    /// GET /Workflow?workflow=Hello%20World&amp;name=Alice
    /// Calls the secure route and renders the raw JSON the engine returns.
    /// </summary>
    // The scopes attribute lets Microsoft.Identity.Web know which delegated
    // scope this action needs, so an incremental-consent challenge requests it.
    [AuthorizeForScopes(ScopeKeySection = "WwExecution:Scopes")]
    public async Task<IActionResult> Index(string workflow = "Hello World", string? name = "Alice", CancellationToken ct = default)
    {
        ViewData["Workflow"] = workflow;
        ViewData["Name"] = name;

        try
        {
            var query = new Dictionary<string, string?> { ["Name"] = name };
            var json = await _engine.ExecuteSecureAsync(workflow, query, ct);
            ViewData["Result"] = json;
        }
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            // Token cache miss / consent required / re-auth — rethrow so the
            // [AuthorizeForScopes] filter converts it into an interactive
            // challenge that redirects the user to Entra ID and back.
            throw;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden
                                              || ex.StatusCode == HttpStatusCode.InternalServerError)
        {
            // The engine rejects roleless users. It currently wraps authorization
            // denials as HTTP 500 (403 path is pending WOLF-8418), so treat both
            // as "not authorized for this workflow".
            _logger.LogWarning(ex, "Engine denied access to workflow {Workflow} (status {Status}).", workflow, ex.StatusCode);
            ViewData["Error"] =
                "The engine denied access. Ensure your account is assigned an app role on the " +
                "engine's resource service principal (roleless users are rejected).";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Engine call failed for workflow {Workflow}.", workflow);
            ViewData["Error"] = $"Engine call failed: {ex.StatusCode?.ToString() ?? ex.Message}";
        }

        return View();
    }
}
