using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Subscription;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Licensing;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Azure Function endpoints for Chargebee subscription registration and status.
    ///
    /// These endpoints allow the Angular web studio to register an Azure Functions
    /// instance by passing the target server address as a parameter.  Business logic
    /// is delegated to the existing <see cref="GetSubscriptionData"/> and
    /// <see cref="SaveSubscriptionData"/> internal service classes — no ESB routing
    /// is required because the classes are instantiated directly with their dependencies.
    ///
    /// Chargebee site (test vs live) is determined at compile time by the existing
    /// <c>#if DEBUG</c> branch in <see cref="SubscriptionConfig"/>.
    ///
    /// Routes:
    ///   GET  /IsLicensed         — anonymous; returns local license status without a Chargebee call
    ///   GET  /Subscriptions      — function-key; refreshes subscription data from Chargebee
    ///   POST /secure/Subscriptions — JWT Bearer; creates a new subscription or links an existing one (Administrator only)
    /// </summary>
    public sealed class LicensingHttpFunction
    {
        readonly IWarewolfLicense _warewolfLicense;
        readonly Dev2JsonSerializer _serializer;
        readonly ILogger<LicensingHttpFunction> _logger;

        public LicensingHttpFunction(IWarewolfLicense warewolfLicense, ILogger<LicensingHttpFunction> logger)
        {
            _warewolfLicense = warewolfLicense;
            _logger = logger;
            _serializer = new Dev2JsonSerializer();
        }

        /// <summary>
        /// Returns whether this instance considers itself licensed, based on the locally
        /// persisted subscription data.  No Chargebee API call is made — this is a fast
        /// local check intended for the Angular page's initial load.
        /// </summary>
        [Function("IsLicensed")]
        public async Task<HttpResponseData> IsLicensed(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "IsLicensed")] HttpRequestData req)
        {
            try
            {
                var data = SubscriptionProvider.Instance.GetSubscriptionData();
                _logger.LogInformation("IsLicensed: isLicensed={IsLicensed}, status={Status}, planId={PlanId}, stopExecutions={StopExecutions}",
                    data.IsLicensed, data.Status?.ToString(), data.PlanId, data.StopExecutions);
                return await ResponseBuilder.BuildStringAsync(req, JsonConvert.SerializeObject(new
                {
                    isLicensed     = data.IsLicensed,
                    status         = data.Status?.ToString(),
                    planId         = data.PlanId,
                    stopExecutions = data.StopExecutions
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IsLicensed: failed to retrieve subscription data");
                return await ResponseBuilder.BuildStringAsync(req, JsonConvert.SerializeObject(new
                {
                    isLicensed     = false,
                    status         = (string)null,
                    planId         = (string)null,
                    stopExecutions = false
                }));
            }
        }

        /// <summary>
        /// Retrieves the current subscription from Chargebee and returns the full
        /// subscription data.  Delegates to <see cref="GetSubscriptionData.Execute"/> which
        /// also updates the local cache if the plan or status has changed.
        /// </summary>
        [Function("GetSubscriptionData")]
        public async Task<HttpResponseData> GetSubscription(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Subscriptions")] HttpRequestData req)
        {
            try
            {
                var svc = new GetSubscriptionData(_warewolfLicense, SubscriptionProvider.Instance);
                var resultSb = svc.Execute(new Dictionary<string, StringBuilder>(), theWorkspace: null);
                var execMsg = _serializer.Deserialize<ExecuteMessage>(resultSb);

                if (execMsg.HasError)
                    return await ErrorResponse(req, execMsg.Message.ToString(), HttpStatusCode.InternalServerError);

                var subscriptionData = _serializer.Deserialize<SubscriptionData>(execMsg.Message);
                return await ResponseBuilder.BuildStringAsync(req, JsonConvert.SerializeObject(subscriptionData));
            }
            catch (Exception ex)
            {
                return await ErrorResponse(req, ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new Chargebee subscription or links an existing one, then persists
        /// the result locally.  Delegates to <see cref="SaveSubscriptionData.Execute"/> for
        /// all business logic: duplicate detection, email validation, and persistence.
        ///
        /// Requires a valid <c>Authorization: Bearer &lt;token&gt;</c> JWT header.  The caller
        /// must belong to the <em>Administrator</em> group; requests without a valid token
        /// are rejected with <c>401 Unauthorized</c>.
        ///
        /// Authentication is handled by the middleware pipeline for this <c>/secure/*</c>
        /// route; FunctionContext is used to retrieve the pre-built principal.
        /// </summary>
        [Function("SaveSubscriptionData")]
        public async Task<HttpResponseData> SaveSubscription(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "secure/Subscriptions")] HttpRequestData req,
            FunctionContext context)
        {
            // Prefer the principal built by the middleware pipeline.
            // Fall back to direct JWT validation for compatibility when middleware is not active.
            var principal = context.Items.TryGetValue(AuthConstants.PrincipalContextKey, out var p)
                ? p as WorkflowClaimsPrincipal
                : null;

            var isAuthenticated = principal?.Identity?.IsAuthenticated == true;

            if (!isAuthenticated)
            {
                var authHeader = req.Headers.TryGetValues("Authorization", out var hvals)
                    ? hvals.FirstOrDefault()
                    : null;
                var config = SecureConfigLoader.Config;
                if (!config.IsLoaded || JwtValidator.GetUserGroups(authHeader, config.SecretKey) is null)
                    return await BuildUnauthorizedResponse(req);
            }

            try
            {
                using var reader = new StreamReader(req.Body);
                var body = await reader.ReadToEndAsync();
                var subscriptionData = JsonConvert.DeserializeObject<SubscriptionData>(body);

                if (subscriptionData is null)
                    return await ErrorResponse(req, "Request body is missing or invalid.", HttpStatusCode.BadRequest);

                var dict = new Dictionary<string, StringBuilder>
                {
                    // Key matches Warewolf.Service.SaveSubscriptionData.SubscriptionData constant.
                    ["SubscriptionData"] = _serializer.SerializeToBuilder(subscriptionData)
                };

                var svc = new SaveSubscriptionData(_serializer, _warewolfLicense, SubscriptionProvider.Instance);
                var resultSb = svc.Execute(dict, theWorkspace: null);
                var execMsg = _serializer.Deserialize<ExecuteMessage>(resultSb);

                if (execMsg.HasError)
                {
                    var message = execMsg.Message.ToString();
                    var statusCode = message.Contains("already exists") ? HttpStatusCode.Conflict : HttpStatusCode.BadRequest;
                    return await ErrorResponse(req, message, statusCode);
                }

                return await ResponseBuilder.BuildStringAsync(req,
                    JsonConvert.SerializeObject(new { success = true }),
                    statusCode: HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return await ErrorResponse(req, ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        static Task<HttpResponseData> ErrorResponse(HttpRequestData req, string message, HttpStatusCode statusCode) =>
            ResponseBuilder.BuildStringAsync(req,
                JsonConvert.SerializeObject(new { hasErrors = true, errors = new[] { message } }),
                statusCode: statusCode);

        static async Task<HttpResponseData> BuildUnauthorizedResponse(HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.Unauthorized);
            response.Headers.Add("WWW-Authenticate", "Bearer");
            await response.WriteStringAsync(JsonConvert.SerializeObject(new
            {
                error = "Authentication required. Provide a valid JWT Bearer token in the Authorization header."
            }));
            return response;
        }
    }
}
