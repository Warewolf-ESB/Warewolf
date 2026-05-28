/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Warewolf.Execution.Lightweight.Http;

/// <summary>
/// Shared helpers for writing structured JSON error responses from Azure Functions middleware and functions.
/// </summary>
public static class HttpResponseHelper
{
    /// <summary>
    /// HTTP header name used to propagate the per-request correlation identifier.
    /// </summary>
    public const string CorrelationIdHeader = "X-WW-Correlation-Id";

    /// <summary>
    /// JSON serialiser options used for flat (camelCase) error responses.
    /// </summary>
    internal static readonly JsonSerializerOptions CamelCaseOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Writes a flat JSON error response of the form:
    /// <code>
    /// {
    ///   "error": "...",
    ///   "message": "...",
    ///   "path": "...",
    ///   "correlationId": "..."
    /// }
    /// </code>
    /// Additional properties from <paramref name="extra"/> are merged in at the top level.
    /// </summary>
    public static async Task WriteErrorAsync(
        HttpRequestData request,
        FunctionContext context,
        HttpStatusCode statusCode,
        string error,
        string message,
        string path,
        string correlationId,
        object? extra = null)
    {
        var body = new Dictionary<string, object>
        {
            ["error"]         = error,
            ["message"]       = message,
            ["path"]          = path,
            ["correlationId"] = correlationId,
        };

        if (extra is not null)
        {
            foreach (var prop in extra.GetType().GetProperties())
                body[prop.Name.ToLowerInvariant()] = prop.GetValue(extra) ?? string.Empty;
        }

        var response = request.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        response.Headers.Add(CorrelationIdHeader, correlationId);
        await response.WriteStringAsync(JsonSerializer.Serialize(body, CamelCaseOptions));
        context.GetInvocationResult().Value = response;
    }

    /// <summary>
    /// Writes a wrapped JSON error response of the form:
    /// <code>
    /// {
    ///   "Error": {
    ///     "Status": 500,
    ///     "Title": "...",
    ///     "Message": "...",
    ///     "Description": "...",
    ///     "CorrelationId": "..."
    ///   }
    /// }
    /// </code>
    /// </summary>
    public static async Task<HttpResponseData> WriteWrappedErrorAsync(
        HttpRequestData request,
        FunctionContext? context,
        HttpStatusCode statusCode,
        int status,
        string title,
        string message,
        string description,
        string correlationId)
    {
        var body = new
        {
            Error = new
            {
                Status        = status,
                Title         = title,
                Message       = message,
                Description   = description,
                CorrelationId = correlationId,
            }
        };

        var response = request.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        response.Headers.Add(CorrelationIdHeader, correlationId);
        await response.WriteStringAsync(JsonSerializer.Serialize(body));
        if (context != null) context.GetInvocationResult().Value = response;
        return response;
    }


    public static string ResolveCorrelationId(HttpRequestData request)
    {
        if (request.Headers.TryGetValues(CorrelationIdHeader, out var values))
        {
            var v = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(v)) return v!;
        }
        // Compact ID — short enough for HTTP headers, unique enough for tracing.
        return Guid.NewGuid().ToString("N")[..16];
    }
}
