/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Tests (8501) for WorkflowHttpFunction.FlushUsagePublishContext — the
 *  bridge that transfers the AsyncLocal UsagePublishContext (populated deep
 *  inside a synchronous WorkflowExecutor.Execute call) into
 *  FunctionContext.Items, where UsagePublishMiddleware can reliably read it
 *  after the rest of the (potentially async) pipeline unwinds.
 *
 *  This bridge exists because AsyncLocal mutations made by a callee are only
 *  guaranteed visible to the caller when nothing async-suspends in between;
 *  FunctionContext.Items is a plain mutable dictionary tied to the
 *  FunctionContext object identity and survives any number of awaits
 *  elsewhere in the pipeline.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Tests.Auth;

namespace Warewolf.Execution.Lightweight.Tests.Functions;

[TestClass]
public class WorkflowHttpFunctionUsagePublishBridgeTests
{
    [TestCleanup]
    public void Cleanup() => UsagePublishContext.Current = null;

    [TestMethod]
    public void FlushUsagePublishContext_AmbientValueSet_MovesItIntoFunctionContextItems()
    {
        var context = new TestFunctionContext();
        var executionId = Guid.NewGuid();
        UsagePublishContext.Current = new UsagePublishContext
        {
            WorkflowName = "Hello World",
            ExecutionId = executionId,
            IsSuccess = true,
            ErrorCount = 0
        };

        WorkflowHttpFunction.FlushUsagePublishContext(context);

        Assert.IsTrue(context.Items.TryGetValue(UsagePublishContext.ItemsKey, out var raw));
        Assert.IsInstanceOfType(raw, typeof(UsagePublishContext));
        var moved = (UsagePublishContext)raw!;
        Assert.AreEqual("Hello World", moved.WorkflowName);
        Assert.AreEqual(executionId, moved.ExecutionId);
    }

    [TestMethod]
    public void FlushUsagePublishContext_ClearsTheAmbientSlotAfterMoving()
    {
        var context = new TestFunctionContext();
        UsagePublishContext.Current = new UsagePublishContext { WorkflowName = "wf", ExecutionId = Guid.NewGuid(), IsSuccess = true };

        WorkflowHttpFunction.FlushUsagePublishContext(context);

        Assert.IsNull(UsagePublishContext.Current,
            "The AsyncLocal slot must be cleared once moved, so a later call on a reused thread cannot see a stale value.");
    }

    [TestMethod]
    public void FlushUsagePublishContext_NoAmbientValue_LeavesItemsUntouched()
    {
        var context = new TestFunctionContext();

        WorkflowHttpFunction.FlushUsagePublishContext(context);

        Assert.IsFalse(context.Items.ContainsKey(UsagePublishContext.ItemsKey),
            "Requests that never reach WorkflowExecutor.Execute must not populate Items with anything for the middleware to publish.");
    }

    [TestMethod]
    public void FlushUsagePublishContext_NullContext_DoesNotThrow_AndClearsAmbientSlot()
    {
        UsagePublishContext.Current = new UsagePublishContext { WorkflowName = "wf", ExecutionId = Guid.NewGuid(), IsSuccess = true };

        WorkflowHttpFunction.FlushUsagePublishContext(null);

        Assert.IsNull(UsagePublishContext.Current);
    }
}
