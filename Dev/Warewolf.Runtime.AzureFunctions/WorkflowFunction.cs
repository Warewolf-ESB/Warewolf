using Dev2.Runtime.ESB.Execution;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security;
using System.Text.Json;

namespace Warewolf.Runtime.AzureFunctions;

public class WorkflowFunction
{
    private readonly ILogger<WorkflowFunction> _log;

    public WorkflowFunction(ILogger<WorkflowFunction> log)
    {
        _log = log;
    }

    [Function("ExecuteWorkflow")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post",
                     Route = "workflow/{workflowName}")]
        HttpRequestData req,
        string workflowName)
    {
        _log.LogInformation("ExecuteWorkflow: '{WorkflowName}' {Method}", workflowName, req.Method);

        try
        {
            // Collect input variables: query string (GET) or JSON/XML body (POST).
            // Query string params become DataList variables, e.g. ?Name=Ash → <DataList><Name>Ash</Name></DataList>.
            // The function key param 'code' is always excluded.
            var inputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var part in req.Url.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = part.IndexOf('=');
                if (eq <= 0) continue;
                var key = Uri.UnescapeDataString(part[..eq]);
                var val = Uri.UnescapeDataString(part[(eq + 1)..]);
                if (!key.Equals("code", StringComparison.OrdinalIgnoreCase))
                    inputs[key] = val;
            }

            string bodyXml = null;
            if (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                var body = await req.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    // Try JSON object → merge keys into inputs
                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        if (doc.RootElement.ValueKind == JsonValueKind.Object)
                            foreach (var prop in doc.RootElement.EnumerateObject())
                                inputs[prop.Name] = prop.Value.ValueKind == JsonValueKind.String
                                    ? prop.Value.GetString()
                                    : prop.Value.GetRawText();
                    }
                    catch
                    {
                        // Not JSON — pass raw body through as-is (may already be DataList XML)
                        bodyXml = body;
                    }
                }
            }

            // Build DataList XML from collected inputs if we don't have a raw XML body
            string inputXml = bodyXml;
            if (inputXml == null && inputs.Count > 0)
            {
                inputXml = "<DataList>"
                    + string.Concat(inputs.Select(kv =>
                        $"<{kv.Key}>{SecurityElement.Escape(kv.Value)}</{kv.Key}>"))
                    + "</DataList>";
            }

            _log.LogInformation("ExecuteWorkflow: inputXml={InputXml}", inputXml ?? "(none)");

            var (resultId, errors, outputs) = AzureFunctionWorkflowRunner.ExecuteWorkflow(workflowName, inputXml);

            var errorList = errors.FetchErrors();
            _log.LogInformation("ExecuteWorkflow: resultId={ResultId} errors={ErrorCount}", resultId, errorList.Count);

            if (errorList.Count > 0)
                foreach (var err in errorList)
                    _log.LogWarning("ExecuteWorkflow: error: {Error}", err);

            var statusCode = errorList.Count == 0 ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            var payload = JsonSerializer.Serialize(new { resultId, errors = errorList, outputs });

            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(payload);
            return response;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "ExecuteWorkflow: unhandled exception for '{WorkflowName}'", workflowName);
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
                _log.LogError("  --> {Type}: {Message}", inner.GetType().FullName, inner.Message);

            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteStringAsync(JsonSerializer.Serialize(new
            {
                resultId = Guid.Empty,
                errors = new[] { ex.Message },
                exceptionType = ex.GetType().FullName,
                stackTrace = ex.StackTrace,
            }));
            return response;
        }
    }
}
