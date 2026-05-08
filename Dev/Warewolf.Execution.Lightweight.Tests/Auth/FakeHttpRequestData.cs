/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Test helper — minimal fake HttpRequestData / HttpResponseData implementations
 *  that work entirely in-memory without spinning up the Functions worker host.
 */

using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

internal sealed class FakeHttpRequestData : HttpRequestData
{
    private readonly HttpHeadersCollection _headers = new();

    public FakeHttpRequestData(FunctionContext context, Uri url, string method = "GET")
        : base(context)
    {
        Url    = url;
        Method = method;
    }

    public override Stream                Body       { get; } = new MemoryStream();
    public override HttpHeadersCollection Headers    => _headers;
    public override IReadOnlyCollection<IHttpCookie> Cookies => Array.Empty<IHttpCookie>();
    public override Uri                   Url        { get; }
    public override IEnumerable<ClaimsIdentity> Identities => Array.Empty<ClaimsIdentity>();
    public override string                Method     { get; }

    public override HttpResponseData CreateResponse() =>
        new FakeHttpResponseData(FunctionContext);

    public void AddHeader(string name, string value) => _headers.Add(name, value);
}

internal sealed class FakeHttpResponseData : HttpResponseData
{
    public FakeHttpResponseData(FunctionContext context) : base(context) { }

    public override HttpStatusCode        StatusCode { get; set; } = HttpStatusCode.OK;
    public override HttpHeadersCollection Headers    { get; set; } = new();
    public override Stream                Body       { get; set; } = new MemoryStream();
    public override HttpCookies           Cookies    { get; } = new FakeHttpCookies();

    private sealed class FakeHttpCookies : HttpCookies
    {
        public override void Append(string name, string value) { }
        public override void Append(IHttpCookie cookie) { }
        public override IHttpCookie CreateNew() =>
            throw new NotSupportedException();
    }
}
