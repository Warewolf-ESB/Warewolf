using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Subscription;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
    ///   GET  /IsLicensed    — anonymous; returns local license status without a Chargebee call
    ///   GET  /Subscriptions — function-key; refreshes subscription data from Chargebee
    ///   POST /Subscriptions — function-key; creates a new subscription or links an existing one
    /// </summary>
    public sealed class LicensingHttpFunction
    {
        readonly IWarewolfLicense _warewolfLicense;
        readonly Dev2JsonSerializer _serializer;

        public LicensingHttpFunction(IWarewolfLicense warewolfLicense)
        {
            _warewolfLicense = warewolfLicense;
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
            var data = SubscriptionProvider.Instance.GetSubscriptionData();
            return await ResponseBuilder.BuildStringAsync(req, JsonConvert.SerializeObject(new
            {
                isLicensed     = data.IsLicensed,
                status         = data.Status?.ToString(),
                planId         = data.PlanId,
                stopExecutions = data.StopExecutions
            }));
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
        /// Request body (JSON):
        /// <code>
        /// // New subscription:
        /// { "CustomerFirstName": "Jane", "CustomerLastName": "Smith",
        ///   "CustomerEmail": "jane@example.com", "PlanId": "warewolf-developer" }
        ///
        /// // Link existing subscription:
        /// { "CustomerEmail": "jane@example.com", "SubscriptionId": "sub_abc123" }
        /// </code>
        ///
        /// The Chargebee API key and site name are always sourced from the locally
        /// persisted secure config — callers cannot override them.
        /// </summary>
        [Function("SaveSubscriptionData")]
        public async Task<HttpResponseData> SaveSubscription(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Subscriptions")] HttpRequestData req)
        {
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
    }
}
