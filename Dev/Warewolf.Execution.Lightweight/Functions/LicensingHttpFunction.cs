using Dev2.Runtime.Subscription;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Warewolf.Licensing;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Azure Function endpoints for Chargebee subscription registration and status.
    ///
    /// These endpoints allow the Angular web studio to register an Azure Functions
    /// instance by passing the target server address as a parameter.  They call the
    /// Chargebee API directly (Option B) — no ESB management-service routing required.
    ///
    /// Routes:
    ///   GET  /IsLicensed      — anonymous; returns local license status from secure.config
    ///   GET  /Subscriptions   — function-key; refreshes subscription data from Chargebee
    ///   POST /Subscriptions   — function-key; creates a new subscription or links an
    ///                           existing one by subscription ID, then persists locally
    /// </summary>
    public sealed class LicensingHttpFunction
    {
        readonly IWarewolfLicense _warewolfLicense;

        public LicensingHttpFunction(IWarewolfLicense warewolfLicense)
        {
            _warewolfLicense = warewolfLicense;
        }

        /// <summary>
        /// Returns whether this instance considers itself licensed, based on the locally
        /// persisted subscription data.  No Chargebee API call is made.
        /// </summary>
        [Function("IsLicensed")]
        public async Task<HttpResponseData> IsLicensed(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "IsLicensed")] HttpRequestData req)
        {
            var provider = SubscriptionProvider.Instance;
            var data = provider.GetSubscriptionData();
            return await ResponseBuilder.BuildStringAsync(req, JsonConvert.SerializeObject(new
            {
                isLicensed    = data.IsLicensed,
                status        = data.Status?.ToString(),
                planId        = data.PlanId,
                stopExecutions = data.StopExecutions
            }));
        }

        /// <summary>
        /// Retrieves the current subscription from Chargebee using the locally stored
        /// subscription ID and credentials, and returns the full subscription data.
        /// Updates the local cache if the plan or status has changed.
        /// </summary>
        [Function("GetSubscriptionData")]
        public async Task<HttpResponseData> GetSubscription(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Subscriptions")] HttpRequestData req)
        {
            try
            {
                var provider = SubscriptionProvider.Instance;

                if (string.IsNullOrEmpty(provider.SubscriptionId))
                {
                    var defaultData = provider.DefaultSubscription();
                    return await ResponseBuilder.BuildStringAsync(req, JsonConvert.SerializeObject(defaultData));
                }

                var subscriptionData = _warewolfLicense.RetrievePlan(
                    provider.SubscriptionId,
                    provider.SubscriptionKey,
                    provider.SubscriptionSiteName);

                if (subscriptionData.PlanId != provider.PlanId ||
                    subscriptionData.Status != provider.Status)
                {
                    provider.SaveSubscriptionData(subscriptionData);
                }

                return await ResponseBuilder.BuildStringAsync(req, JsonConvert.SerializeObject(subscriptionData));
            }
            catch (Exception ex)
            {
                return await ResponseBuilder.BuildStringAsync(req,
                    JsonConvert.SerializeObject(new { hasErrors = true, errors = new[] { ex.Message } }),
                    statusCode: HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Creates a new Chargebee subscription or links an existing one.
        ///
        /// Request body (JSON):
        /// <code>
        /// {
        ///   "CustomerFirstName": "Jane",
        ///   "CustomerLastName":  "Smith",
        ///   "CustomerEmail":     "jane@example.com",
        ///   "PlanId":            "warewolf-developer",
        ///   "NoOfCores":         4,
        ///
        ///   // Supply SubscriptionId to link an existing subscription instead of creating one:
        ///   "SubscriptionId": ""
        /// }
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
                {
                    return await ResponseBuilder.BuildStringAsync(req,
                        JsonConvert.SerializeObject(new { hasErrors = true, errors = new[] { "Request body is missing or invalid." } }),
                        statusCode: HttpStatusCode.BadRequest);
                }

                // Always use the locally stored Chargebee credentials — never trust the caller's.
                var provider = SubscriptionProvider.Instance;
                subscriptionData.SubscriptionKey      = provider.SubscriptionKey;
                subscriptionData.SubscriptionSiteName = provider.SubscriptionSiteName;

                // Guard against duplicate subscriptions for the same customer.
                if (string.IsNullOrEmpty(subscriptionData.SubscriptionId) &&
                    _warewolfLicense.SubscriptionExists(subscriptionData))
                {
                    return await ResponseBuilder.BuildStringAsync(req,
                        JsonConvert.SerializeObject(new { hasErrors = true, errors = new[] { "A subscription already exists for this customer." } }),
                        statusCode: HttpStatusCode.Conflict);
                }

                ISubscriptionData result;
                if (string.IsNullOrEmpty(subscriptionData.SubscriptionId))
                {
                    result = _warewolfLicense.CreatePlan(subscriptionData);
                }
                else
                {
                    result = _warewolfLicense.RetrievePlan(
                        subscriptionData.SubscriptionId,
                        subscriptionData.SubscriptionKey,
                        subscriptionData.SubscriptionSiteName);
                }

                if (result is null)
                {
                    return await ResponseBuilder.BuildStringAsync(req,
                        JsonConvert.SerializeObject(new { hasErrors = true, errors = new[] { "An error occurred creating the subscription." } }),
                        statusCode: HttpStatusCode.InternalServerError);
                }

                if (!string.IsNullOrEmpty(subscriptionData.CustomerEmail) &&
                    result.CustomerEmail != subscriptionData.CustomerEmail)
                {
                    return await ResponseBuilder.BuildStringAsync(req,
                        JsonConvert.SerializeObject(new { hasErrors = true, errors = new[] { "Email address does not match the subscription." } }),
                        statusCode: HttpStatusCode.BadRequest);
                }

                provider.SaveSubscriptionData(result);

                return await ResponseBuilder.BuildStringAsync(req,
                    JsonConvert.SerializeObject(result),
                    statusCode: HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return await ResponseBuilder.BuildStringAsync(req,
                    JsonConvert.SerializeObject(new { hasErrors = true, errors = new[] { ex.Message } }),
                    statusCode: HttpStatusCode.InternalServerError);
            }
        }
    }
}
