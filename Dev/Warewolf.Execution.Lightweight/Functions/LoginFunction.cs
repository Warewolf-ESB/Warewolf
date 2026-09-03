/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Functions
{
    /// <summary>
    /// Azure Function that mirrors the Warewolf server's <c>/login</c> route.
    ///
    /// Accepts a POST request with credentials in the JSON body, executes the
    /// configured <c>AuthenticationOverrideWorkflow</c>, validates that the workflow
    /// output contains a non-empty <c>UserGroups</c> array, and returns a signed
    /// HMAC-SHA256 JWT token.
    ///
    /// <para>
    /// The POST body is treated as a flat JSON object whose keys map directly to
    /// DataList <b>Input</b> scalar names in the login workflow, for example:
    /// <code>
    ///   POST /login
    ///   { "Username": "alice", "Password": "secret" }
    /// </code>
    /// </para>
    ///
    /// <para>Response codes:</para>
    /// <list type="bullet">
    ///   <item><c>200</c> — <c>{"token":"eyJ..."}</c></item>
    ///   <item><c>400</c> — workflow output missing or invalid <c>UserGroups</c></item>
    ///   <item><c>401</c> — workflow execution failed (errors present)</item>
    ///   <item><c>501</c> — no login workflow configured in <c>secure.config</c></item>
    /// </list>
    /// </summary>
    public sealed class LoginFunction
    {
        readonly IWorkflowExecutor _workflowExecutor;
        readonly string            _workflowsDirectory;

        public LoginFunction(IWorkflowExecutor workflowExecutor, Infrastructure.HostEnvironmentConfig hostEnvironmentConfig = null)
        {
            _workflowExecutor   = workflowExecutor;
            // WOLF-8516: no env-var fallback — WorkflowsDirectory comes solely from
            // HostEnvironmentConfig (deploy-bundled settings file), which DI always supplies.
            _workflowsDirectory = hostEnvironmentConfig?.WorkflowsDirectory
                ?? Path.Combine(AppContext.BaseDirectory, "Resources");
        }

        [Function("Login")]
        public async Task<HttpResponseData> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "login")] HttpRequestData req)
        {
            var config = SecureConfigLoader.Config;

            // ── 1. Guard: login workflow must be configured ───────────────────────
            if (string.IsNullOrWhiteSpace(config.LoginWorkflowName))
            {
                return await BuildErrorAsync(req, HttpStatusCode.NotImplemented,
                    "No login workflow configured. Set AuthenticationOverrideWorkflow in secure.config.");
            }

            // ── 2. Parse credentials from POST body ───────────────────────────────
            var inputs = await ParseCredentialsAsync(req);

            // ── 3. Execute the login workflow ─────────────────────────────────────
            var executionRequest = WorkflowFunctionHelper.CreateRequestByName(
                config.LoginWorkflowName,
                _workflowsDirectory,
                inputs);

            var result = _workflowExecutor.Execute(executionRequest);

            if (!result.IsSuccess || result.Errors?.Count > 0)
            {
                var errorMsg = result.Errors?.FirstOrDefault() ?? "Login workflow execution failed.";
                return await BuildErrorAsync(req, HttpStatusCode.Unauthorized, errorMsg);
            }

            // ── 4. Extract and validate UserGroups from workflow output ────────────
            // Use ReadPayloadAsync because the executor stores output in PayloadWriter
            // (a streaming delegate), not in the Payload string property.
            var payload = await result.ReadPayloadAsync();
            var userGroups = ExtractUserGroups(payload);
            if (userGroups is null || userGroups.Count == 0 || userGroups.Any(string.IsNullOrWhiteSpace))
            {
                // Include the raw payload so callers can diagnose their login workflow's DataList output.
                var truncated = string.IsNullOrWhiteSpace(payload)
                    ? "(empty)"
                    : payload.Length > 500 ? payload[..500] + "…" : payload;
                return await BuildErrorAsync(req, HttpStatusCode.BadRequest,
                    $"invalid login override workflow selected: outputs not valid. Workflow returned: {truncated}");
            }

            // ── 5. Generate JWT and return ────────────────────────────────────────
            var token = JwtGenerator.GenerateToken(userGroups, config.SecretKey);
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonConvert.SerializeObject(new { token }));
            return response;
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds input parameters for the login workflow from the HTTP request.
        ///
        /// Query string parameters are read first, then POST body JSON properties are
        /// merged on top (body takes precedence when the same key appears in both).
        /// This supports both:
        /// <list type="bullet">
        ///   <item>GET  <c>/login?Username=alice&amp;Password=x</c></item>
        ///   <item>POST <c>/login</c> with <c>{"Username":"alice","Password":"x"}</c></item>
        /// </list>
        /// </summary>
        static async Task<Dictionary<string, string>> ParseCredentialsAsync(HttpRequestData req)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // ── 1. Query string parameters (base layer) ───────────────────────────
            if (req.Url != null)
            {
                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
                foreach (string key in query)
                {
                    if (!string.IsNullOrWhiteSpace(key))
                        result[key] = query[key] ?? string.Empty;
                }
            }

            // ── 2. POST body JSON (overrides query string on collision) ───────────
            if (req.Body != null && req.Body.CanRead)
            {
                try
                {
                    using var reader = new StreamReader(req.Body);
                    var body = await reader.ReadToEndAsync();

                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        var obj = JObject.Parse(body);
                        foreach (var prop in obj.Properties())
                        {
                            result[prop.Name] = prop.Value.Type == JTokenType.String
                                ? prop.Value.Value<string>() ?? string.Empty
                                : prop.Value.ToString();
                        }
                    }
                }
                catch
                {
                    // Body was not JSON — keep whatever query string params we already have.
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts the <c>UserGroups</c> string array from the workflow output payload.
        ///
        /// The login workflow must output JSON containing a <c>UserGroups</c> array.
        /// Two formats are accepted:
        /// <list type="bullet">
        ///   <item>String array: <c>{"UserGroups":["Admins","Users"]}</c></item>
        ///   <item>Object array (full-server format): <c>{"UserGroups":[{"Name":"Admins"}]}</c></item>
        /// </list>
        /// Returns <c>null</c> when the payload is absent, not JSON, or lacks a
        /// <c>UserGroups</c> property.
        /// </summary>
        static IReadOnlyList<string>? ExtractUserGroups(string? payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return null;

            try
            {
                var obj = JObject.Parse(payload);
                var groupsToken = obj["UserGroups"];
                if (groupsToken is not JArray arr)
                    return null;

                var groups = new List<string>(arr.Count);
                foreach (var item in arr)
                {
                    if (item.Type == JTokenType.String)
                    {
                        groups.Add(item.Value<string>() ?? string.Empty);
                    }
                    else if (item is JObject groupObj)
                    {
                        // Accept full-server object format: {"Name": "Admins"}
                        var name = groupObj["Name"]?.Value<string>();
                        if (name != null)
                            groups.Add(name);
                    }
                }

                return groups;
            }
            catch
            {
                return null;
            }
        }

        static async Task<HttpResponseData> BuildErrorAsync(
            HttpRequestData req, HttpStatusCode statusCode, string message)
        {
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(
                JsonConvert.SerializeObject(new { error = message }));
            return response;
        }
    }
}
