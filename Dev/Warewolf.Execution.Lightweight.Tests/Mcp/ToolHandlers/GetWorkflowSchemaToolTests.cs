/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for GetWorkflowSchemaTool: verifies the four static schema documents are present,
 *  stable across calls, and shaped per the spec's envelope_schema/body_schema/
 *  add_step_schema descriptions, and that test_schema lists the value sets create_test enforces.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text.Json;
using Dev2.Common.Interfaces;
using Dev2.Data.Decisions.Operations;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class GetWorkflowSchemaToolTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ReturnsAllFourSchemas_AsJsonObjects()
        {
            var result = GetWorkflowSchemaTool.Handle();

            Assert.AreEqual(JsonValueKind.Object, result.EnvelopeSchema.ValueKind);
            Assert.AreEqual(JsonValueKind.Object, result.BodySchema.ValueKind);
            Assert.AreEqual(JsonValueKind.Object, result.AddStepSchema.ValueKind);
            Assert.AreEqual(JsonValueKind.Object, result.TestSchema.ValueKind);
        }

        /// <summary>
        /// create_test/edit_test point callers at test_schema for the shape of their `test` argument, so it must
        /// document every TestDefinitionModel property.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_TestSchema_HasEveryTestDefinitionProperty()
        {
            var schema = GetWorkflowSchemaTool.Handle().TestSchema;

            foreach (var name in new[] { "testName", "enabled", "authenticationType", "userName", "password", "inputs", "outputs", "noErrorExpected", "errorExpected", "errorContainsText", "testSteps" })
            {
                Assert.IsTrue(schema.TryGetProperty(name, out _), $"test_schema should document `{name}`.");
            }
            var step = schema.GetProperty("testSteps")[0];
            foreach (var name in new[] { "activityId", "activityType", "type", "stepDescription", "stepOutputs", "children" })
            {
                Assert.IsTrue(step.TryGetProperty(name, out _), $"test_schema testSteps should document `{name}`.");
            }
        }

        /// <summary>The documented closed value sets match the enums create_test and execute_test parse against.</summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_TestSchema_ListsEveryAuthenticationTypeStepTypeAndAssertOp()
        {
            var schema = GetWorkflowSchemaTool.Handle().TestSchema;
            var authenticationTypes = schema.GetProperty("authenticationType").GetString()!;
            var stepType = schema.GetProperty("testSteps")[0].GetProperty("type").GetString()!;
            var assertOp = schema.GetProperty("outputs")[0].GetProperty("assertOp").GetString()!;

            foreach (var name in Enum.GetNames<AuthenticationType>())
            {
                StringAssert.Contains(authenticationTypes, name);
            }
            foreach (var name in Enum.GetNames<StepType>())
            {
                StringAssert.Contains(stepType, name);
            }
            foreach (var value in Enum.GetValues<enDecisionType>().Where(v => v != enDecisionType.Choose))
            {
                StringAssert.Contains(assertOp, DecisionDisplayHelper.GetDisplayValue(value));
            }
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

        /// <summary>
        /// F7: shape:"edge" is mandatory on an edge cell (ValidateWorkflowTool/
        /// X6ToWorkflowConverter both identify edges purely by it) but was previously undocumented
        /// here, leaving the edge cell looking identical to {source, target, label?} with no clue
        /// that shape was required.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_BodySchema_EdgeCell_DocumentsRequiredShape()
        {
            var result = GetWorkflowSchemaTool.Handle();

            var cells = result.BodySchema.GetProperty("cells");
            var edgeCell = cells.EnumerateArray().FirstOrDefault(c => c.TryGetProperty("source", out _));

            Assert.AreNotEqual(JsonValueKind.Undefined, edgeCell.ValueKind, "body_schema should document an edge cell shape.");
            Assert.IsTrue(edgeCell.TryGetProperty("shape", out var shape));
            StringAssert.Contains(shape.GetString(), "edge");
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
            Assert.AreEqual(first.TestSchema.GetRawText(), second.TestSchema.GetRawText());
        }
    }
}
