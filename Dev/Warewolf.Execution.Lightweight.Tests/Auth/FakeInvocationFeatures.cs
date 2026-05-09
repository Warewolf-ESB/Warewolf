/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Minimal IInvocationFeatures implementation for unit tests.
 *  Returns null (the default) for every Get<T>() call, which causes
 *  FunctionContext.GetHttpRequestDataAsync() to return null — the behaviour
 *  expected for non-HTTP triggers (timers, queues, etc.).
 *  Use Set<T>() to register specific features when a test needs them.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

internal sealed class FakeInvocationFeatures : IInvocationFeatures
{
    private readonly Dictionary<Type, object> _store = new();

    public FakeInvocationFeatures()
    {
        // Register a null-returning IHttpRequestDataFeature so the SDK's
        // DefaultHttpRequestDataFeature fallback (which requires FunctionDefinition)
        // is never reached, and GetHttpRequestDataAsync() safely returns null.
        Set<IHttpRequestDataFeature>(NullHttpRequestDataFeature.Instance);
    }

    public T? Get<T>() =>
        _store.TryGetValue(typeof(T), out var v) ? (T)v : default;

    public void Set<T>(T instance) =>
        _store[typeof(T)] = instance!;

    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() =>
        _store.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class NullHttpRequestDataFeature : IHttpRequestDataFeature
    {
        public static readonly NullHttpRequestDataFeature Instance = new();
        public ValueTask<HttpRequestData?> GetHttpRequestDataAsync(FunctionContext context)
            => new(default(HttpRequestData));
    }
}
