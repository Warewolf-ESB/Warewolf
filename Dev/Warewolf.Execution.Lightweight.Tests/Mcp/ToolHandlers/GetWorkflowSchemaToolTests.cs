/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for GetWorkflowSchemaTool: verifies the three static schema documents are present,
 *  stable across calls, and shaped per the spec's envelope_schema/body_schema/
 *  add_step_schema descriptions.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class GetWorkflowSchemaToolTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ReturnsAllThreeSchemas_AsJsonObjects()
        {
            var result = GetWorkflowSchemaTool.Handle();

            Assert.AreEqual(JsonValueKind.Object, result.EnvelopeSchema.ValueKind);
            Assert.AreEqual(JsonValueKind.Object, result.BodySchema.ValueKind);
            Assert.AreEqual(JsonValueKind.Object, result.AddStepSchema.ValueKind);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_EnvelopeSchema_HasNameDescriptionInputsOutputs()
        {
            var result = GetWorkflowSchemaTool.Handle();

            Assert.IsTrue(result.EnvelopeSchema.TryGetProperty("name", out _));
            Assert.IsTrue(result.EnvelopeSchema.TryGetProperty("description", out _));
            Assert.IsTrue(result.EnvelopeSchema.TryGetProperty("inputs", out var inputs));
            Assert.IsTrue(result.EnvelopeSchema.TryGetProperty("outputs", out var outputs));
            Assert.AreEqual(JsonValueKind.Array, inputs.ValueKind);
            Assert.AreEqual(JsonValueKind.Array, outputs.ValueKind);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_BodySchema_HasResourcenameAndCells()
        {
            var result = GetWorkflowSchemaTool.Handle();

            Assert.IsTrue(result.BodySchema.TryGetProperty("resourcename", out _));
            Assert.IsTrue(result.BodySchema.TryGetProperty("cells", out var cells));
            Assert.AreEqual(JsonValueKind.Array, cells.ValueKind);
            Assert.IsTrue(cells.GetArrayLength() >= 2, "cells should document both a node shape and an edge shape.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_AddStepSchema_HasShapeDataAfterStepIdBranch()
        {
            var result = GetWorkflowSchemaTool.Handle();

            Assert.IsTrue(result.AddStepSchema.TryGetProperty("shape", out _));
            Assert.IsTrue(result.AddStepSchema.TryGetProperty("data", out _));
            Assert.IsTrue(result.AddStepSchema.TryGetProperty("afterStepId", out _));
            Assert.IsTrue(result.AddStepSchema.TryGetProperty("branch", out _));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_IsStableAcrossCalls()
        {
            var first = GetWorkflowSchemaTool.Handle();
            var second = GetWorkflowSchemaTool.Handle();

            Assert.AreEqual(first.EnvelopeSchema.GetRawText(), second.EnvelopeSchema.GetRawText());
            Assert.AreEqual(first.BodySchema.GetRawText(), second.BodySchema.GetRawText());
            Assert.AreEqual(first.AddStepSchema.GetRawText(), second.AddStepSchema.GetRawText());
        }
    }
}
