/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Warewolf.Execution.Lightweight.Auth.Models;
using HttpCookie = Microsoft.Azure.Functions.Worker.Http.HttpCookie;

namespace Warewolf.Execution.Lightweight.SelfHost;

/// <summary>
/// An ASP.NET Core request as the Functions <see cref="HttpRequestData"/> the engine's functions take.
/// <para>
/// The body is buffered, as the Functions host does, so middleware and the function can each read it.
/// The Easy Auth principal headers are dropped: under Azure, App Service strips any a client sends and
/// sets its own after validating the sign-in, but nothing does that here, so honouring them would let
/// any caller claim any identity.
/// </para>
/// </summary>
internal sealed class SelfHostHttpRequestData : HttpRequestData
{
    private SelfHostHttpRequestData(FunctionContext context, HttpRequest request, MemoryStream body)
        : base(context)
    {
        Body = body;
        Method = request.Method;
        Url = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}{request.QueryString}");
        Headers = new HttpHeadersCollection();
        foreach (var header in request.Headers)
        {
            if (IsEasyAuthHeader(header.Key)) continue;
            Headers.Add(header.Key, header.Value.Where(value => value is not null).Select(value => value!));
        }
        Cookies = request.Cookies.Select(cookie => (IHttpCookie)new HttpCookie(cookie.Key, cookie.Value)).ToList();
    }

    internal static async Task<SelfHostHttpRequestData> CreateAsync(FunctionContext context, HttpRequest request)
    {
        var body = new MemoryStream();
        await request.Body.CopyToAsync(body, request.HttpContext.RequestAborted);
        body.Position = 0;
        return new SelfHostHttpRequestData(context, request, body);
    }

    /// <summary><c>X-MS-CLIENT-PRINCIPAL</c> and the rest of App Service's <c>X-MS-CLIENT-PRINCIPAL-*</c> family.</summary>
    internal static bool IsEasyAuthHeader(string name) =>
        name.StartsWith(AuthConstants.ClientPrincipalHeader, StringComparison.OrdinalIgnoreCase);

    public override Stream Body { get; }
    public override HttpHeadersCollection Headers { get; }
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
    public override Uri Url { get; }
    public override IEnumerable<ClaimsIdentity> Identities => Array.Empty<ClaimsIdentity>();
    public override string Method { get; }

    public override HttpResponseData CreateResponse() => new SelfHostHttpResponseData(FunctionContext);
}

internal sealed class SelfHostHttpResponseData(FunctionContext context) : HttpResponseData(context)
{
    private readonly SelfHostHttpCookies _cookies = new();

    public override HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public override HttpHeadersCollection Headers { get; set; } = new();
    public override Stream Body { get; set; } = new MemoryStream();
    public override HttpCookies Cookies => _cookies;

    internal IReadOnlyList<IHttpCookie> AppendedCookies => _cookies.Appended;
}

internal sealed class SelfHostHttpCookies : HttpCookies
{
    internal List<IHttpCookie> Appended { get; } = new();

    public override void Append(string name, string value) => Appended.Add(new HttpCookie(name, value));
    public override void Append(IHttpCookie cookie) => Appended.Add(cookie);
    public override IHttpCookie CreateNew() => new HttpCookie(string.Empty, string.Empty);
}
