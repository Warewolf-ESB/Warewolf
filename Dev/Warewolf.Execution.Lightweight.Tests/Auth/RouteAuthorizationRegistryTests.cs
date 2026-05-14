/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class RouteAuthorizationRegistryTests
{
    public class FunctionsA
    {
        [Function("DecoratedA")]
        [RequireWorkflowPermission(WorkflowPermission.View | WorkflowPermission.Execute)]
        public Task DecoratedA() => Task.CompletedTask;

        [Function("AdminOnlyA")]
        [RequireWorkflowPermission(WorkflowPermission.Administrator)]
        public Task AdminOnly() => Task.CompletedTask;

        [Function("UndecoratedA")]
        public Task Undecorated() => Task.CompletedTask;
    }

    public class FunctionsB
    {
        [Function("DecoratedB")]
        [RequireWorkflowPermission(WorkflowPermission.Contribute)]
        public Task DecoratedB() => Task.CompletedTask;
    }

    [TestMethod]
    public void TST05_AttributePresent_ReturnsConfiguredPermissions()
    {
        var registry = RouteAuthorizationRegistry.BuildFrom(typeof(FunctionsA));

        Assert.AreEqual(
            WorkflowPermission.View | WorkflowPermission.Execute,
            registry.GetRequiredPermissions("DecoratedA"));
        Assert.AreEqual(
            WorkflowPermission.Administrator,
            registry.GetRequiredPermissions("AdminOnlyA"));
    }

    [TestMethod]
    public void TST05_AttributeAbsent_ReturnsNull()
    {
        var registry = RouteAuthorizationRegistry.BuildFrom(typeof(FunctionsA));
        Assert.IsNull(registry.GetRequiredPermissions("UndecoratedA"));
    }

    [TestMethod]
    public void TST05_UnknownFunctionName_ReturnsNull()
    {
        var registry = RouteAuthorizationRegistry.BuildFrom(typeof(FunctionsA));
        Assert.IsNull(registry.GetRequiredPermissions("DoesNotExist"));
    }

    [TestMethod]
    public void TST05_MultipleTypesCombined_AllAttributesAggregated()
    {
        var registry = RouteAuthorizationRegistry.BuildFrom(typeof(FunctionsA), typeof(FunctionsB));

        Assert.AreEqual(WorkflowPermission.View | WorkflowPermission.Execute, registry.GetRequiredPermissions("DecoratedA"));
        Assert.AreEqual(WorkflowPermission.Contribute, registry.GetRequiredPermissions("DecoratedB"));
    }

    [TestMethod]
    public void TST05_FunctionNameLookup_IsCaseInsensitive()
    {
        var registry = RouteAuthorizationRegistry.BuildFrom(typeof(FunctionsA));

        Assert.AreEqual(
            WorkflowPermission.View | WorkflowPermission.Execute,
            registry.GetRequiredPermissions("DECORATEDA"));
        Assert.AreEqual(
            WorkflowPermission.View | WorkflowPermission.Execute,
            registry.GetRequiredPermissions("decorateda"));
    }
}
