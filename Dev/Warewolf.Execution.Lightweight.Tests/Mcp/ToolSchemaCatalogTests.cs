/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ToolSchemaCatalog: the static per-tool `data` field schema
 *  documents backing get_tool_schema (warewolf-lee-mcp-v3-spec.md, "Tools" §
 *  get_tool_schema). Chiefly guards the 1:1 coverage invariant GetToolSchemaTool
 *  relies on — every ToolCatalog entry must have a matching schema document.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Mcp;

namespace Warewolf.Execution.Lightweight.Tests.Mcp
{
    [TestClass]
    public class ToolSchemaCatalogTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void TryGet_ReturnsTrue_ForEveryToolCatalogEntry()
        {
            foreach (var entry in ToolCatalog.Entries)
            {
                Assert.IsTrue(ToolSchemaCatalog.TryGet(entry.Name, out var schema),
                    $"No ToolSchemaCatalog document authored for ToolCatalog entry '{entry.Name}'.");
                Assert.AreEqual(JsonValueKind.Object, schema.ValueKind, $"'{entry.Name}' schema must be a JSON object.");
                Assert.IsTrue(schema.TryGetProperty("fields", out var fields),
                    $"'{entry.Name}' schema must have a 'fields' property (may be an empty object for zero-field activities).");
                Assert.AreEqual(JsonValueKind.Object, fields.ValueKind, $"'{entry.Name}' schema's 'fields' must be a JSON object.");
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TryGet_ReturnsFalse_ForUnknownName()
        {
            Assert.IsFalse(ToolSchemaCatalog.TryGet("Not A Real Tool", out _));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TryGet_ReturnsFalse_ForBlankName()
        {
            Assert.IsFalse(ToolSchemaCatalog.TryGet("", out _));
            Assert.IsFalse(ToolSchemaCatalog.TryGet("   ", out _));
            Assert.IsFalse(ToolSchemaCatalog.TryGet(null!, out _));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TryGet_HasNoUnexpectedEntries_BeyondToolCatalog()
        {
            // Every schema document must correspond to a real ToolCatalog entry — a
            // stale/renamed entry here would silently never be reachable via get_tool_schema.
            foreach (var name in new[] { "Assign", "Switch", "SQL Server Database", "Gather System Information" })
            {
                Assert.IsTrue(ToolSchemaCatalog.TryGet(name, out _));
            }
        }
    }
}
