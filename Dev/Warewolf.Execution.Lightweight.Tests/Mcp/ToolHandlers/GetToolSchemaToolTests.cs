/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for GetToolSchemaTool: verifies lookup-by-name semantics (case-insensitive match
 *  against ToolCatalog.Entry.Name, distinct from ToolCatalog.Resolve's
 *  data.type substring match), the tool_name/activity_type/dataType passthrough,
 *  error behavior for unknown/empty names, and that control-flow tools document
 *  branching/nesting per spec.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using System.Linq;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class GetToolSchemaToolTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ReturnsSchema_ForEveryToolCatalogEntry()
        {
            foreach (var entry in ToolCatalog.Entries)
            {
                var result = GetToolSchemaTool.Handle(entry.Name);

                Assert.AreEqual(entry.Name, result.ToolName);
                Assert.AreEqual(entry.ActivityType, result.ActivityType);
                Assert.AreEqual(entry.DataTypes[0], result.DataType);
                Assert.AreEqual(JsonValueKind.Object, result.Schema.ValueKind,
                    $"'{entry.Name}' schema must be a JSON object.");
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_MatchesByName_NotByDataType()
        {
            // "Assign" the Studio name must resolve; the underlying dataType alias string
            // ("dsfdotnetmultiassignactivity") must NOT also work as a tool_name — get_tool_schema
            // keys off list_tools' `name`, unlike ToolCatalog.Resolve which keys off `dataType`.
            var byName = GetToolSchemaTool.Handle("Assign");
            Assert.AreEqual("Assign", byName.ToolName);

            Assert.ThrowsException<McpException>(() => GetToolSchemaTool.Handle("dsfdotnetmultiassignactivity"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_IsCaseInsensitive()
        {
            var lower = GetToolSchemaTool.Handle("assign");
            var upper = GetToolSchemaTool.Handle("ASSIGN");
            var mixed = GetToolSchemaTool.Handle("aSsIgN");

            Assert.AreEqual("Assign", lower.ToolName);
            Assert.AreEqual("Assign", upper.ToolName);
            Assert.AreEqual("Assign", mixed.ToolName);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Throws_ForUnknownToolName()
        {
            var ex = Assert.ThrowsException<McpException>(() => GetToolSchemaTool.Handle("Not A Real Tool"));
            StringAssert.Contains(ex.Message, "not a recognized Warewolf tool");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Throws_ForEmptyToolName()
        {
            Assert.ThrowsException<McpException>(() => GetToolSchemaTool.Handle(""));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Throws_ForWhitespaceToolName()
        {
            Assert.ThrowsException<McpException>(() => GetToolSchemaTool.Handle("   "));
        }

        [DataTestMethod]
        [TestCategory("UnitTest")]
        [DataRow("Decision")]
        [DataRow("Decision (legacy)")]
        [DataRow("Switch")]
        public void Handle_DecisionAndSwitch_DocumentBranching(string toolName)
        {
            var result = GetToolSchemaTool.Handle(toolName);

            Assert.IsTrue(result.Schema.TryGetProperty("branching", out _),
                $"'{toolName}' is a branching control-flow tool and must document its branching mechanism.");
        }

        [DataTestMethod]
        [TestCategory("UnitTest")]
        [DataRow("Sequence")]
        [DataRow("For Each")]
        [DataRow("Select and apply")]
        public void Handle_ContainerTools_DocumentNesting(string toolName)
        {
            var result = GetToolSchemaTool.Handle(toolName);

            Assert.IsTrue(result.Schema.TryGetProperty("nesting", out _),
                $"'{toolName}' nests child activities and must document the isNested/parentId mechanism.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_AssignSchema_DocumentsFieldsShape()
        {
            var result = GetToolSchemaTool.Handle("Assign");

            Assert.IsTrue(result.Schema.TryGetProperty("fields", out var fields));
            Assert.IsTrue(fields.TryGetProperty("fields", out var fieldsDoc));
            StringAssert.Contains(fieldsDoc.GetString(), "FieldName");
            StringAssert.Contains(fieldsDoc.GetString(), "FieldValue");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SqlServerDatabaseSchema_DocumentsSpecCalledOutFields()
        {
            var result = GetToolSchemaTool.Handle("SQL Server Database");

            Assert.IsTrue(result.Schema.TryGetProperty("fields", out var fields));
            foreach (var expected in new[] { "procedurename", "executeactionstring", "isOutputToObject", "objectname" })
            {
                Assert.IsTrue(fields.TryGetProperty(expected, out _), $"SQL Server Database schema must document '{expected}'.");
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_RabbitMqConsumeSchema_DocumentsSpecCalledOutFields()
        {
            var result = GetToolSchemaTool.Handle("RabbitMQ Consume");

            Assert.IsTrue(result.Schema.TryGetProperty("fields", out var fields));
            foreach (var expected in new[] { "queuename", "isobject", "prefetch", "acknowledge", "requeue" })
            {
                Assert.IsTrue(fields.TryGetProperty(expected, out _), $"RabbitMQ Consume schema must document '{expected}'.");
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_IsStableAcrossCalls()
        {
            var first = GetToolSchemaTool.Handle("Assign");
            var second = GetToolSchemaTool.Handle("Assign");

            Assert.AreEqual(first.Schema.GetRawText(), second.Schema.GetRawText());
        }
    }
}
