/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Warewolf.Execution.Lightweight.SelfHost;

/// <summary>
/// One of the engine's HTTP functions, read from its <c>[Function]</c> and <c>[HttpTrigger]</c>
/// attributes so the self-hosted engine serves exactly the routes and methods the Functions host does.
/// </summary>
internal sealed class HttpFunction
{
    private HttpFunction(MethodInfo method, string name, string route, string[] methods)
    {
        Method = method;
        Route = route;
        Methods = methods;
        Definition = new SelfHostFunctionDefinition(name, $"{method.DeclaringType!.FullName}.{method.Name}", method.DeclaringType.Assembly.Location);
    }

    internal MethodInfo Method { get; }
    /// <summary>The ASP.NET Core route template, as the Functions host reads it (host.json's routePrefix is empty).</summary>
    internal string Route { get; }
    /// <summary>Upper-case HTTP methods; empty means any.</summary>
    internal string[] Methods { get; }
    internal FunctionDefinition Definition { get; }

    /// <summary>Every HTTP-triggered function in <paramref name="assembly"/>. The Service Bus triggers aren't HTTP and don't run self-hosted.</summary>
    internal static IReadOnlyList<HttpFunction> Discover(Assembly assembly) =>
        assembly.GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Select(method => (method, function: method.GetCustomAttribute<FunctionAttribute>()))
            .Where(pair => pair.function is not null)
            .Select(pair =>
            {
                var trigger = pair.method.GetParameters().Select(p => p.GetCustomAttribute<HttpTriggerAttribute>()).FirstOrDefault(a => a is not null);
                return trigger is null
                    ? null
                    : new HttpFunction(
                        pair.method,
                        pair.function!.Name,
                        trigger.Route ?? pair.function.Name,
                        (trigger.Methods ?? Array.Empty<string>()).Select(m => m.ToUpperInvariant()).ToArray());
            })
            .Where(function => function is not null)
            .Select(function => function!)
            .OrderBy(function => function.Route, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Calls the function as the Functions host would: an instance from DI (constructor injection), the
    /// trigger's <see cref="HttpRequestData"/>, the <see cref="FunctionContext"/>, the cancellation token, and
    /// route values bound to parameters by name.
    /// </summary>
    internal async Task<HttpResponseData?> InvokeAsync(SelfHostFunctionContext context)
    {
        var instance = Method.IsStatic ? null : ActivatorUtilities.GetServiceOrCreateInstance(context.InstanceServices, Method.DeclaringType!);
        var arguments = Method.GetParameters().Select(parameter => Bind(parameter, context)).ToArray();

        object? result;
        try
        {
            result = Method.Invoke(instance, arguments);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }

        switch (result)
        {
            case Task<HttpResponseData> typed:
                return await typed;
            case Task task:
                await task;
                return null;
            default:
                return result as HttpResponseData;
        }
    }

    private static object? Bind(ParameterInfo parameter, SelfHostFunctionContext context)
    {
        var type = parameter.ParameterType;
        if (typeof(HttpRequestData).IsAssignableFrom(type)) return context.Request;
        if (typeof(FunctionContext).IsAssignableFrom(type)) return context;
        if (type == typeof(CancellationToken)) return context.CancellationToken;

        if (parameter.Name is not null
            && context.BindingContext.BindingData.TryGetValue(parameter.Name, out var value)
            && value is not null)
        {
            if (type.IsInstanceOfType(value)) return value;
            var converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(value.GetType())) return converter.ConvertFrom(value);
        }
        return parameter.HasDefaultValue ? parameter.DefaultValue : type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
