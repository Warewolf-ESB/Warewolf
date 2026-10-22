/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Warewolf.Execution.Lightweight.SelfHost;

namespace Warewolf.Execution.Lightweight.Http;

/// <summary>
/// Sets the HTTP response a middleware or helper decided on, short-circuiting the function.
/// <para>
/// Under the Functions host that is <c>GetInvocationResult().Value</c>, which reads an internal
/// worker feature. The self-hosted engine (<see cref="SelfHostedEngine"/>) has no such feature, so its
/// <see cref="SelfHostFunctionContext"/> holds the response itself.
/// </para>
/// </summary>
internal static class FunctionContextResponseExtensions
{
    internal static void SetHttpResponse(this FunctionContext context, HttpResponseData response)
    {
        if (context is SelfHostFunctionContext selfHosted)
            selfHosted.Response = response;
        else
            context.GetInvocationResult().Value = response;
    }
}
