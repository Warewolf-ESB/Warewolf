/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Minimal in-memory FunctionContext used by parser and helper tests.
 *  Only the surface members the unit-tests actually touch are implemented.
 */

using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

internal sealed class TestFunctionContext : FunctionContext
{
    private readonly Dictionary<object, object?> _items = new();
    private readonly IServiceProvider _services = new ServiceCollection().BuildServiceProvider();

    public override string                       InvocationId       => "test-invocation";
    public override string                       FunctionId         => "test-function";
    public override TraceContext                 TraceContext       => null!;
    public override BindingContext               BindingContext     => null!;
    public override RetryContext                 RetryContext       => null!;
    public override IServiceProvider             InstanceServices   { get => _services; set { } }
    public override FunctionDefinition           FunctionDefinition => null!;
    public override IDictionary<object, object?> Items              { get => _items; set { } }
    public override IInvocationFeatures          Features           => null!;
}
