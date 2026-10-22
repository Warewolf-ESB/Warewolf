/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Warewolf.Execution.Lightweight.SelfHost;

/// <summary>
/// The <see cref="FunctionContext"/> a request gets under the self-hosted engine
/// (<see cref="SelfHostedEngine"/>): enough of the Functions worker's for the engine's middleware and
/// functions to run unchanged. <c>GetHttpRequestDataAsync()</c> works through
/// <see cref="IHttpRequestDataFeature"/>; a response set by middleware lands in <see cref="Response"/>
/// (see <c>FunctionContextResponseExtensions.SetHttpResponse</c>).
/// </summary>
internal sealed class SelfHostFunctionContext : FunctionContext
{
    private readonly SelfHostInvocationFeatures _features = new();

    internal SelfHostFunctionContext(HttpFunction function, IServiceProvider services, IReadOnlyDictionary<string, object?> bindingData, CancellationToken cancellationToken)
    {
        FunctionDefinition = function.Definition;
        InstanceServices = services;
        BindingContext = new SelfHostBindingContext(bindingData);
        CancellationToken = cancellationToken;
        var activity = Activity.Current;
        TraceContext = new SelfHostTraceContext(activity?.Id ?? string.Empty, activity?.TraceStateString ?? string.Empty);
    }

    /// <summary>The request, set once its body is buffered.</summary>
    internal SelfHostHttpRequestData? Request
    {
        get => _request;
        set
        {
            _request = value;
            _features.Set<IHttpRequestDataFeature>(new RequestFeature(value));
        }
    }
    private SelfHostHttpRequestData? _request;

    /// <summary>The response a middleware short-circuited with, or the function returned.</summary>
    internal HttpResponseData? Response { get; set; }

    public override string InvocationId { get; } = Guid.NewGuid().ToString();
    public override string FunctionId => FunctionDefinition.Id;
    public override TraceContext TraceContext { get; }
    public override BindingContext BindingContext { get; }
    public override RetryContext RetryContext { get; } = new SelfHostRetryContext();
    public override IServiceProvider InstanceServices { get; set; }
    public override FunctionDefinition FunctionDefinition { get; }
    public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
    public override IInvocationFeatures Features => _features;
    public override CancellationToken CancellationToken { get; }

    private sealed class RequestFeature(SelfHostHttpRequestData? request) : IHttpRequestDataFeature
    {
        public ValueTask<HttpRequestData?> GetHttpRequestDataAsync(FunctionContext context) => new(request);
    }
}

internal sealed class SelfHostInvocationFeatures : IInvocationFeatures
{
    private readonly Dictionary<Type, object> _features = new();

    public void Set<T>(T instance)
    {
        if (instance is null) _features.Remove(typeof(T));
        else _features[typeof(T)] = instance;
    }

    public T Get<T>() => _features.TryGetValue(typeof(T), out var feature) ? (T)feature : default!;

    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() => _features.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal sealed class SelfHostBindingContext(IReadOnlyDictionary<string, object?> bindingData) : BindingContext
{
    public override IReadOnlyDictionary<string, object?> BindingData { get; } = bindingData;
}

internal sealed class SelfHostTraceContext(string traceParent, string traceState) : TraceContext
{
    public override string TraceParent { get; } = traceParent;
    public override string TraceState { get; } = traceState;
}

/// <summary>HTTP functions are never retried; only the Service Bus triggers are, and those don't run self-hosted.</summary>
internal sealed class SelfHostRetryContext : RetryContext
{
    public override int RetryCount => 0;
    public override int MaxRetryCount => 0;
}

internal sealed class SelfHostFunctionDefinition(string name, string entryPoint, string pathToAssembly) : FunctionDefinition
{
    public override ImmutableArray<FunctionParameter> Parameters => ImmutableArray<FunctionParameter>.Empty;
    public override string PathToAssembly { get; } = pathToAssembly;
    public override string EntryPoint { get; } = entryPoint;
    public override string Id { get; } = name;
    public override string Name { get; } = name;
    public override IImmutableDictionary<string, BindingMetadata> InputBindings => ImmutableDictionary<string, BindingMetadata>.Empty;
    public override IImmutableDictionary<string, BindingMetadata> OutputBindings => ImmutableDictionary<string, BindingMetadata>.Empty;
}
