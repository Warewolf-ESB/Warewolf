/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Test helper — implements the SDK-internal IFunctionBindingsFeature via
 *  DispatchProxy so that FunctionContext.GetInvocationResult().Value = response
 *  can be exercised in unit tests without spinning up the Functions worker host.
 *
 *  Usage:
 *      var bindings = FakeBindingsFeature.Create();
 *      features.Set<IFunctionBindingsFeature>(bindings.Proxy);
 *      // ... invoke middleware ...
 *      var captured = bindings.InvocationResult; // the HttpResponseData that was set
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Azure.Functions.Worker;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

/// <summary>
/// Captures whatever is written to <c>IFunctionBindingsFeature.InvocationResult</c>
/// by production code that calls <c>context.GetInvocationResult().Value = response</c>.
/// </summary>
internal sealed class FakeBindingsFeature
{
    // The SDK-internal interface type, resolved once.
    private static readonly Type s_featureType =
        typeof(FunctionContext).Assembly
            .GetType("Microsoft.Azure.Functions.Worker.Context.Features.IFunctionBindingsFeature", throwOnError: true)!;

    // The proxy object that implements IFunctionBindingsFeature at runtime.
    public object Proxy { get; private set; }

    // The value last written to InvocationResult (typically an HttpResponseData).
    public object? InvocationResult { get; private set; }

    private FakeBindingsFeature()
    {
        Proxy = null!; // assigned immediately in Create() before returning
    }

    /// <summary>Creates a new <see cref="FakeBindingsFeature"/> and its proxy.</summary>
    public static FakeBindingsFeature Create()
    {
        var fake  = new FakeBindingsFeature();
        var proxy = DispatchProxyCreate(s_featureType, fake);
        fake.Proxy = proxy;
        return fake;
    }

    // Registers the proxy with FakeInvocationFeatures under the internal interface type.
    public void Register(FakeInvocationFeatures features) =>
        features.SetByType(s_featureType, Proxy);

    // ── DispatchProxy plumbing ────────────────────────────────────────────────

    // We cannot inherit DispatchProxy<IFunctionBindingsFeature> because the interface
    // is internal, so we use the generic overload via reflection.
    private static object DispatchProxyCreate(Type interfaceType, FakeBindingsFeature owner)
    {
        // DispatchProxy.Create<TProxy, TDecorator>() — both must be concrete types.
        // We use BindingsProxy as the decorator and IFunctionBindingsFeature as TProxy.
        // Use GetMethods() + LINQ because GetMethod() throws AmbiguousMatchException
        // when multiple overloads share the same name.
        var createMethod = typeof(DispatchProxy)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(DispatchProxy.Create)
                      && m.IsGenericMethodDefinition
                      && m.GetGenericArguments().Length == 2
                      && m.GetParameters().Length == 0)
            .MakeGenericMethod(interfaceType, typeof(BindingsProxy));

        var proxy = createMethod.Invoke(null, null)!;

        // Inject owner reference so Invoke() can delegate to it.
        ((BindingsProxy)proxy).Owner = owner;
        return proxy;
    }

    /// <summary>
    /// DispatchProxy subclass that intercepts <c>InvocationResult</c> get/set.
    /// All other property accesses return safe defaults.
    /// Must not be sealed — DispatchProxy.Create() generates a subclass of this type.
    /// </summary>
    public class BindingsProxy : DispatchProxy
    {
        internal FakeBindingsFeature Owner = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null) return null;

            return targetMethod.Name switch
            {
                "set_InvocationResult" => SetInvocationResult(args?[0]),
                "get_InvocationResult" => Owner.InvocationResult,
                "get_TriggerMetadata"  => (object?)new Dictionary<string, object>(),
                "get_InputData"        => (object?)new Dictionary<string, object>(),
                "get_OutputBindingData"=> (object?)new Dictionary<string, object>(),
                "get_OutputBindingsInfo" => null,
                _ => null,
            };
        }

        private object? SetInvocationResult(object? value)
        {
            Owner.InvocationResult = value;
            return null;
        }
    }
}
