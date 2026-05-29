/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Test helper — a FunctionContext that serves a real FakeHttpRequestData
 *  so middleware Invoke() HTTP paths can be exercised in unit tests
 *  without spinning up the Functions worker host.
 *
 *  Use SetHttpRequest() to inject the request before calling Invoke().
 *  Use SetFunctionName() when the test reaches code that reads
 *  FunctionContext.FunctionDefinition.Name (e.g. RouteAuthorizationRegistry
 *  lookups inside WorkflowAuthorizationMiddleware).
 */

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

/// <summary>
/// A <see cref="FunctionContext"/> that can serve an HTTP request and expose a
/// configurable function name.  All other members delegate to safe defaults.
/// </summary>
internal sealed class HttpFunctionContext : FunctionContext
{
    private readonly Dictionary<object, object?> _items    = new();
    private readonly IServiceProvider             _services = new ServiceCollection().BuildServiceProvider();
    private readonly FakeInvocationFeatures       _features = new();
    private readonly FakeBindingsFeature          _bindings = FakeBindingsFeature.Create();
    private FunctionDefinition?                   _def;

    /// <summary>
    /// The last value written to <c>context.GetInvocationResult().Value</c> by
    /// middleware under test.  Typically an <see cref="Microsoft.Azure.Functions.Worker.Http.HttpResponseData"/>.
    /// </summary>
    public object? CapturedInvocationResult => _bindings.InvocationResult;

    public HttpFunctionContext()
    {
        _bindings.Register(_features);
    }

    // ── FunctionContext overrides ──────────────────────────────────────────────

    public override string                       InvocationId       => "test-http-invocation";
    public override string                       FunctionId         => "test-http-function";
    public override TraceContext                 TraceContext       => null!;
    public override BindingContext               BindingContext     => null!;
    public override RetryContext                 RetryContext       => null!;
    public override IServiceProvider             InstanceServices   { get => _services; set { } }
    public override FunctionDefinition           FunctionDefinition => _def ?? new NamedFunctionDefinition("unknown");
    public override IDictionary<object, object?> Items              { get => _items; set { } }
    public override IInvocationFeatures          Features           => _features;

    // ── Configuration helpers for tests ──────────────────────────────────────

    /// <summary>
    /// Registers <paramref name="request"/> so that
    /// <see cref="FunctionContext.GetHttpRequestDataAsync"/> returns it.
    /// </summary>
    public void SetHttpRequest(FakeHttpRequestData request)
        => _features.Set<IHttpRequestDataFeature>(new DirectRequestFeature(request));

    /// <summary>
    /// Overrides <see cref="FunctionDefinition.Name"/> returned by this context.
    /// Call before invoking middleware that reads
    /// <see cref="FunctionContext.FunctionDefinition"/>.Name.
    /// </summary>
    public void SetFunctionName(string name) => _def = new NamedFunctionDefinition(name);

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Returns the injected <see cref="HttpRequestData"/> unconditionally.
    /// </summary>
    private sealed class DirectRequestFeature : IHttpRequestDataFeature
    {
        private readonly HttpRequestData _req;
        public DirectRequestFeature(HttpRequestData req) => _req = req;
        public ValueTask<HttpRequestData?> GetHttpRequestDataAsync(FunctionContext context) => new(_req);
    }

    /// <summary>Minimal <see cref="FunctionDefinition"/> with a configurable name.</summary>
    private sealed class NamedFunctionDefinition : FunctionDefinition
    {
        private readonly string _name;
        public NamedFunctionDefinition(string name) => _name = name;

        public override string                                            Name              => _name;
        public override string                                            Id                => "test-id";
        public override string                                            EntryPoint        => "Test.Entry";
        public override string                                            PathToAssembly    => "test.dll";
        public override IImmutableDictionary<string, BindingMetadata>    InputBindings     => ImmutableDictionary<string, BindingMetadata>.Empty;
        public override IImmutableDictionary<string, BindingMetadata>    OutputBindings    => ImmutableDictionary<string, BindingMetadata>.Empty;
        public override ImmutableArray<FunctionParameter>                Parameters        => ImmutableArray<FunctionParameter>.Empty;
    }
}
