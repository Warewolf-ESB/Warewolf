using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace WwExecutionCaller.Middleware;

/// <summary>
/// Global safety net for unhandled exceptions across every function invocation.
///
/// For HTTP-triggered functions (<see cref="Functions.CallWorkflowOnHttpTrigger"/>), any exception
/// that escapes the function body is converted into an <c>HTTP 500</c> response instead of an
/// opaque host-level failure:
///   • Default — a short <c>{ExceptionType}: {Message}</c> body.
///   • Detailed — the full <see cref="Exception.ToString"/> (type, message, stack trace, inner
///     exceptions) when the request's query string contains a <c>showerror</c> flag
///     (e.g. <c>?showerror</c> or <c>?showerror=true</c>; presence is enough, value is ignored).
///
/// For non-HTTP triggers (e.g. <see cref="Functions.CallWorkflowOnTimer"/>) there is no response to
/// write, so the exception is logged and swallowed — this is a redundant defense-in-depth net,
/// since that function already catches and logs its own exceptions.
/// </summary>
public sealed class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, "Unhandled exception in function '{FunctionName}'.", context.FunctionDefinition.Name);

            var request = await context.GetHttpRequestDataAsync().ConfigureAwait(false);
            if (request is null)
            {
                // Non-HTTP trigger — nothing to write a response to; already logged above.
                return;
            }

            var response = request.CreateResponse(HttpStatusCode.InternalServerError);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            var body = ShowDetailedError(request.Url)
                ? ex.ToString()
                : $"{ex.GetType().Name}: {ex.Message}";

            await response.WriteStringAsync(body).ConfigureAwait(false);
            context.GetInvocationResult().Value = response;
        }
    }

    /// <summary>True when the request's query string contains a <c>showerror</c> key (any value).</summary>
    private static bool ShowDetailedError(Uri url)
    {
        var query = url.Query;
        if (string.IsNullOrEmpty(query))
        {
            return false;
        }

        return query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(pair => pair.Split('=')[0])
            .Any(key => string.Equals(key, "showerror", StringComparison.OrdinalIgnoreCase));
    }
}
