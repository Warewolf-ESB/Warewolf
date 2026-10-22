/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Http;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.SelfHost;

namespace Warewolf.Execution.Lightweight.Tests.SelfHost;

// A packaged engine runs on a VPS beside WolfClaw without the Azure Functions host: the same
// functions and middleware on Kestrel (SelfHostedEngine).
[TestClass]
public class SelfHostedEngineTests
{
    [TestMethod]
    [TestCategory("UnitTest")]
    public void Discover_FindsTheEnginesHttpFunctionsWithTheirRoutesAndMethods_AndNoServiceBusTriggers()
    {
        var functions = HttpFunction.Discover(typeof(SelfHostedEngine).Assembly);

        var listWorkflows = functions.Single(f => f.Route == "mcp-api/list_workflows");
        CollectionAssert.AreEqual(new[] { "POST" }, listWorkflows.Methods);
        Assert.IsTrue(functions.Any(f => f.Route == "Public/{*name}"));
        Assert.IsTrue(functions.Any(f => f.Route == "IsLicensed"));
        Assert.IsTrue(functions.All(f => f.Method.GetParameters().Any(p => typeof(HttpRequestData).IsAssignableFrom(p.ParameterType))));
        Assert.AreEqual(functions.Count, functions.Select(f => f.Definition.Name).Distinct().Count(), "function names are unique");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ApplyDefaults_SetsTheDataDirectoryFreeLicensingAndLoopback_WithoutOverridingWhatsSet()
    {
        var env = new Dictionary<string, string?> { ["WAREWOLF_SELFHOST_URLS"] = "http://127.0.0.1:4000" };
        SelfHostedEngine.ApplyDefaults(name => env.GetValueOrDefault(name), (name, value) => env[name] = value, isWindows: false);

        Assert.AreEqual("/var/lib/warewolf", env["WAREWOLF_DATA_DIR"]);
        Assert.AreEqual("false", env["WAREWOLF_LICENSE_CHECK_ENABLED"]);
        Assert.AreEqual("http://127.0.0.1:4000", env["WAREWOLF_SELFHOST_URLS"]);
        Assert.AreEqual(Path.Combine(@"D:\PD", "Warewolf"), SelfHostedEngine.DefaultDataDirectory(true, name => name == "ProgramData" ? @"D:\PD" : null));
        Assert.AreEqual("http://127.0.0.1:3142", SelfHostedEngine.DefaultUrls);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsRequested_ByFlagOrEnvironment()
    {
        Assert.IsTrue(SelfHostedEngine.IsRequested(new[] { "--self-host" }, _ => null));
        Assert.IsTrue(SelfHostedEngine.IsRequested(Array.Empty<string>(), name => name == "WAREWOLF_SELF_HOST" ? "true" : null));
        Assert.IsFalse(SelfHostedEngine.IsRequested(Array.Empty<string>(), name => name == "WAREWOLF_SELF_HOST" ? "0" : null));
        Assert.IsFalse(SelfHostedEngine.IsRequested(Array.Empty<string>(), _ => null));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void WritableDirectoryFor_DataDirectory_IsItsResourcesFolder()
    {
        Assert.AreEqual(Path.Combine("/var/lib/warewolf", "Resources"), PackagedResources.WritableDirectoryFor(name => name == "WAREWOLF_DATA_DIR" ? "/var/lib/warewolf" : null));
    }

    // Under Azure, App Service strips a client's X-MS-CLIENT-PRINCIPAL and sets its own after
    // validating the sign-in. Nothing does that self-hosted, so the header would let anyone claim
    // any identity.
    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Request_DropsEasyAuthPrincipalHeaders_KeepsTheRest_AndBuffersTheBody()
    {
        var http = new DefaultHttpContext();
        http.Request.Method = "POST";
        http.Request.Scheme = "http";
        http.Request.Host = new HostString("127.0.0.1", 3142);
        http.Request.Path = "/mcp-api/list_workflows";
        http.Request.QueryString = new QueryString("?a=1");
        http.Request.Headers["X-MS-CLIENT-PRINCIPAL"] = "eyJ9";
        http.Request.Headers["X-MS-CLIENT-PRINCIPAL-NAME"] = "admin@example.com";
        http.Request.Headers["Authorization"] = "Bearer t";
        http.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"pageSize\":1}"));

        var function = HttpFunction.Discover(typeof(SelfHostedEngine).Assembly).First();
        var context = new SelfHostFunctionContext(function, new ServiceCollection().BuildServiceProvider(), new Dictionary<string, object?>(), default);
        var request = await SelfHostHttpRequestData.CreateAsync(context, http.Request);

        Assert.IsFalse(request.Headers.Contains("X-MS-CLIENT-PRINCIPAL"));
        Assert.IsFalse(request.Headers.Contains("X-MS-CLIENT-PRINCIPAL-NAME"));
        Assert.IsTrue(request.Headers.Contains("Authorization"));
        Assert.AreEqual("http://127.0.0.1:3142/mcp-api/list_workflows?a=1", request.Url.ToString());
        Assert.AreEqual("{\"pageSize\":1}", await new StreamReader(request.Body).ReadToEndAsync());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Handle_RunsTheMiddlewareInOrderThenTheFunction_BindingRouteValues()
    {
        var calls = new List<string>();
        var http = NewContext(calls, "/echo/wolfclaw");
        http.Request.RouteValues["who"] = "wolfclaw";

        await SelfHostedEngine.HandleAsync(http, EchoFunction(), new[] { typeof(FirstMiddleware), typeof(SecondMiddleware) });

        CollectionAssert.AreEqual(new[] { "first", "second", "function:wolfclaw" }, calls);
        Assert.AreEqual(201, http.Response.StatusCode);
        Assert.AreEqual("echo", http.Response.Headers["X-Test"].ToString());
        http.Response.Body.Position = 0;
        Assert.AreEqual("hello wolfclaw", await new StreamReader(http.Response.Body).ReadToEndAsync());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Handle_AMiddlewaresResponseShortCircuitsTheFunction()
    {
        var calls = new List<string>();
        var http = NewContext(calls, "/echo/x");

        await SelfHostedEngine.HandleAsync(http, EchoFunction(), new[] { typeof(RefusingMiddleware), typeof(FirstMiddleware) });

        CollectionAssert.AreEqual(new[] { "refused" }, calls);
        Assert.AreEqual(401, http.Response.StatusCode);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Write_NoResponse_IsNoContent()
    {
        var http = new DefaultHttpContext();
        await SelfHostedEngine.WriteAsync(http.Response, null);
        Assert.AreEqual(204, http.Response.StatusCode);
    }

    static HttpFunction EchoFunction() => HttpFunction.Discover(typeof(SelfHostedEngineTests).Assembly).Single(f => f.Route == "echo/{who}");

    static DefaultHttpContext NewContext(List<string> calls, string path)
    {
        var services = new ServiceCollection()
            .AddSingleton(calls)
            .AddSingleton<FirstMiddleware>()
            .AddSingleton<SecondMiddleware>()
            .AddSingleton<RefusingMiddleware>()
            .BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        http.Request.Method = "GET";
        http.Request.Scheme = "http";
        http.Request.Host = new HostString("127.0.0.1", 3142);
        http.Request.Path = path;
        http.Response.Body = new MemoryStream();
        return http;
    }

    sealed class FirstMiddleware(List<string> calls) : IFunctionsWorkerMiddleware
    {
        public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            calls.Add("first");
            return next(context);
        }
    }

    sealed class SecondMiddleware(List<string> calls) : IFunctionsWorkerMiddleware
    {
        public Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            calls.Add("second");
            return next(context);
        }
    }

    sealed class RefusingMiddleware(List<string> calls) : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            calls.Add("refused");
            var request = await context.GetHttpRequestDataAsync();
            context.SetHttpResponse(request!.CreateResponse(HttpStatusCode.Unauthorized));
        }
    }
}

public class SelfHostTestFunctions(List<string> calls)
{
    [Function("Echo")]
    public async Task<HttpResponseData> Echo([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "echo/{who}")] HttpRequestData request, string who)
    {
        calls.Add($"function:{who}");
        var response = request.CreateResponse(HttpStatusCode.Created);
        response.Headers.Add("X-Test", "echo");
        await response.WriteStringAsync($"hello {who}");
        return response;
    }
}
